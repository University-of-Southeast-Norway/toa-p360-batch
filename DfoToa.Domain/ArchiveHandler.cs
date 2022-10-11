using dfo_toa_manual.DFO;
using DfoClient;
using Ks.Fiks.Maskinporten.Client;
using Newtonsoft.Json.Linq;
using P360Client.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Runtime.Serialization;
using System.Security;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

namespace DfoToa.Domain
{
    public class ArchiveHandler
    {
        private bool _initiated;
        private MaskinportenToken _token;
        private Client _dfoClient;

        public IContext Context { get; }

        public ArchiveHandler(IContext context)
        {
            Context = context;
        }

        public async Task Archive(IEmployeeContractHandler employeeContractHandler, DateTimeOffset from, DateTimeOffset to)
        {
            await init();

            List<string> contractSequenceList = await GetContractsFromDfo(from, to);

            Console.WriteLine($"Prosesserer {contractSequenceList.Count} kontrakter...");
            Context.CurrentLogger.WriteToLog($"Prosesserer {contractSequenceList.Count} kontrakter...");

            foreach (string contractSequenceNumber in contractSequenceList)
            {
                if (_token.IsExpiring()) await Reset();
                Contract contract = await _dfoClient.GetContractAsync(contractSequenceNumber);
                string message = $"Arbdeider med {contract.SequenceNumber};{contract.ContractId};{contract.EmployeeId}";
                Console.WriteLine(message);
                Context.CurrentLogger.WriteToLog(message);

                try
                {
                    EmployeeContract employeeContract = await _dfoClient.GetEmployeeContractAsync(contract.EmployeeId, contract.ContractId);
                    Console.WriteLine($"Fant {employeeContract}");
                    Context.CurrentLogger.WriteToLog($"Fant {employeeContract}");
                    Employee employee = await _dfoClient.GetEmployee(employeeContract.Id);
                    Console.WriteLine($"Fant {employee}");
                    Context.CurrentLogger.WriteToLog($"Fant {employee}");

                    await employeeContractHandler.RunAsync(employee, contract);
                }
                catch (Exception ex)
                {
                    Context.CurrentLogger.WriteToLog(ex);
                    Console.WriteLine($"Uhåndtert feil har oppstått:{Environment.NewLine}{ex}");
                }
            }
        }

        public async Task Archive(IEmployeeContractHandler employeeContractHandler, List<string> contractSequenceList)
        {
            await init();

            Console.WriteLine($"Prosesserer {contractSequenceList.Count} avtaler...");
            Context.CurrentLogger.WriteToLog($"Prosesserer {contractSequenceList.Count} avtaler...");

            for (int i = 0; i < contractSequenceList.Count; i++)
            {
                if (_token.IsExpiring()) await Reset();
                string contractSequenceNumber = contractSequenceList[i];
                Contract contract = await _dfoClient.GetContractAsync(contractSequenceNumber);
                string message = $"Jobber med {contract.SequenceNumber};{contract.ContractId};{contract.EmployeeId}";
                Console.WriteLine(message);
                Context.CurrentLogger.WriteToLog(message);

                try
                {
                    EmployeeContract employeeContract = await _dfoClient.GetEmployeeContractAsync(contract.EmployeeId, contract.ContractId);
                    Console.WriteLine($"Fant avtale {employeeContract}");
                    Context.CurrentLogger.WriteToLog($"Fant avtale {employeeContract}");
                    Employee employee = await _dfoClient.GetEmployee(employeeContract.Id);
                    Console.WriteLine($"Fant ansatt {employee}");
                    Context.CurrentLogger.WriteToLog($"Fant ansatt {employee}");

                    await employeeContractHandler.RunAsync(employee, contract);
                }
                catch (Exception ex)
                {
                    Context.CurrentLogger.WriteToLog(ex);
                    Console.WriteLine($"Uhåndtert feil har oppstått:{Environment.NewLine}{ex}");
                }
                Console.WriteLine($"Avtaler prosessert: {i + 1} av {contractSequenceList.Count}");
            }
        }

        public async Task<List<string>> GetContractsFromDfo(DateTimeOffset from, DateTimeOffset to)
        {
            await init();
            return await _dfoClient.GetContractSequenceList(from, to);
        }

        private async Task Reset()
        {
            _dfoClient = null;
            await init();
        }

        private async Task init()
        {
            if (_dfoClient != null) return;

            Context.CurrentLogger.WriteToLog("Initierer...");

            var certificate = new X509Certificate2(
                Context.MaskinportenCertificatePath,
                Context.MaskinportenCertificatePassword
            );

            var configuration = new MaskinportenClientConfiguration(
                audience: Context.MaskinportenAudience,
                tokenEndpoint: Context.MaskinportenTokenEndpoint,
                issuer: Context.MaskinportenIssuer,
                numberOfSecondsLeftBeforeExpire: 10,
                certificate: certificate);

            var maskinportenClient = new MaskinportenClient(configuration);

            Context.CurrentLogger.WriteToLog("Henter token fra Maskinporten...");
            _token = await maskinportenClient.GetAccessToken(Context.MaskinportenScope);
            Context.CurrentLogger.WriteToLog($"Token mottatt... ({_token.Token.Substring(0, 10)}...)");
            _dfoClient = new Client(Context.DfoApiBaseAddress, _token.Token);
        }
    }
}
