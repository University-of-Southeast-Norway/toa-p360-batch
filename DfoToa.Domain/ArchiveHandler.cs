using dfo_toa_manual.DFO;
using Ks.Fiks.Maskinporten.Client;
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

namespace DfoClient.Domain
{
    public class ArchiveHandler
    {
        public IContext Context { get; }

        public ArchiveHandler(IContext context)
        {
            Context = context;
        }

        public async Task Archive(IEmployeeContractHandler employeeContractHandler, DateTimeOffset from, DateTimeOffset to)
        {
            await init().ContinueWith(async t =>
            {
                var token = t.Result;
                Context.CurrentLogger.WriteToLog($"Token received... ({token.Token.Substring(10)}...)");
                var dfoClient = new DfoClient.Client(Context.DfoApiBaseAddress, token.Token);

                List<string> contractSequenceList = await dfoClient.GetContractSequenceList(from, to);

                Console.WriteLine($"Prosesserer {contractSequenceList.Count} kontrakter...");
                Context.CurrentLogger.WriteToLog($"Processing {contractSequenceList.Count} contracts");

                foreach (string contractSequenceNumber in contractSequenceList)
                {
                    Contract contract = await dfoClient.GetContractAsync(contractSequenceNumber);
                    Console.WriteLine($"Working on {contract.SequenceNumber};{contract.ContractId};{contract.EmployeeId}");

                    try
                    {
                        EmployeeContract employeeContract = await dfoClient.GetEmployeeContractAsync(contract.EmployeeId, contract.ContractId);
                        Console.WriteLine($"Found {employeeContract}");
                        Employee employee = await dfoClient.GetEmployee(employeeContract.Id);
                        Console.WriteLine($"Found {employee}");

                        await employeeContractHandler.RunAsync(employee, contract);
                    }
                    catch (Exception ex) { Console.WriteLine($"Unhandled error occured:{Environment.NewLine}{ex}"); }
                }
            }).Unwrap();
        }

        protected async Task<MaskinportenToken> init()
        {
            Context.CurrentLogger.WriteToLog("Initializing...");

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

            Context.CurrentLogger.WriteToLog("Get access token from Maskinporten...");
            return await maskinportenClient.GetAccessToken(Context.MaskinportenScope);
        }
    }
}
