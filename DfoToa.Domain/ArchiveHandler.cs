using DfoClient;
using Ks.Fiks.Maskinporten.Client;
using System.Security.Cryptography.X509Certificates;

namespace DfoToa.Domain;

public class ArchiveHandler
{
    private MaskinportenToken? _token;
    private Client? _dfoClient;

    public IContext Context { get; }

    public ArchiveHandler(IContext context)
    {
        Context = context;
    }

    public async Task Archive(IEmployeeContractHandler employeeContractHandler, List<string> contractSequenceList)
    {
        Console.WriteLine($"Prosesserer {contractSequenceList.Count} avtaler...");
        Context.CurrentLogger.WriteToLog($"Prosesserer {contractSequenceList.Count} avtaler...");

        for (int i = 0; i < contractSequenceList.Count; i++)
        {
            string contractSequenceNumber = contractSequenceList[i];
            await Archive(employeeContractHandler, contractSequenceNumber);
            Console.WriteLine($"Avtaler prosessert: {i + 1} av {contractSequenceList.Count}");
        }
    }

    public async Task Archive(IEmployeeContractHandler employeeContractHandler, string contractSequenceNumber)
    {
        Client client = await GetOrCreateDfoClient();
        if (!Context.UseApiKey && _token!.IsExpiring()) await Reset();
        Contract contract = await client.GetContractAsync(contractSequenceNumber);
        string message = $"Jobber med {contract.SequenceNumber};{contract.ContractId};{contract.EmployeeId}";
        Console.WriteLine(message);
        Context.CurrentLogger.WriteToLog(message);

        try
        {
            EmployeeContract employeeContract = await client.GetEmployeeContractAsync(contract.EmployeeId, contract.ContractId);
            Console.WriteLine($"Fant avtale {employeeContract}");
            Context.CurrentLogger.WriteToLog($"Fant avtale {employeeContract}");
            Employee employee = await client.GetEmployeeAsync(employeeContract.Id, Context.SearchDate);
            Console.WriteLine($"Fant ansatt {employee}");
            Context.CurrentLogger.WriteToLog($"Fant ansatt {employee}");

            await employeeContractHandler.RunAsync(employee, contract);
        }
        catch (Exception ex)
        {
            Context.CurrentLogger.WriteToLog(ex);
            Console.WriteLine($"Uhåndtert feil har oppstått:{Environment.NewLine}{ex}");
        }
    }

    public async Task<List<string>> GetContractsFromDfo(DateTimeOffset from, DateTimeOffset to)
    {
        Client client = await GetOrCreateDfoClient();
        return await client.GetContractSequenceListAsync(from, to);
    }

    private async Task Reset()
    {
        _dfoClient = null;
        _dfoClient = await GetOrCreateDfoClient();
    }

    private async Task<Client> GetOrCreateDfoClient()
    {
        if (_dfoClient != null) return _dfoClient;

        Context.CurrentLogger.WriteToLog("Initierer...");

        if (Context.UseApiKey)
        {
            _dfoClient = new ApiKeyClient(Context.DfoApiBaseAddress, Context.ApiKeyProvider);
        }
        else
        {
            var certificate = new X509Certificate2(
                Context.MaskinportenCertificatePath,
                Context.MaskinportenCertificatePassword
            );

            var configuration = new MaskinportenClientConfiguration(
                audience: Context.MaskinportenAudience,
                tokenEndpoint: Context.MaskinportenTokenEndpoint,
                issuer: Context.MaskinportenIssuer,
                numberOfSecondsLeftBeforeExpire: 119,
                certificate: certificate);

            var maskinportenClient = new MaskinportenClient(configuration);

            Context.CurrentLogger.WriteToLog("Henter token fra Maskinporten...");
            _token = await maskinportenClient.GetAccessToken(Context.MaskinportenScope);
            Context.CurrentLogger.WriteToLog($"Token mottatt... ({_token.Token.Substring(0, 10)}...)");
            _dfoClient = new JwtAuthorizationClient(Context.DfoApiBaseAddress, Context.TokenResolver);
        }
        return _dfoClient;
    }
}
