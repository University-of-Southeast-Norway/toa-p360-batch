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
            Context.CurrentLogger.WriteToLog($"Processing {contractSequenceList.Count} contracts");

            foreach (string contractSequenceNumber in contractSequenceList)
            {
                Contract contract = await _dfoClient.GetContractAsync(contractSequenceNumber);
                Console.WriteLine($"Working on {contract.SequenceNumber};{contract.ContractId};{contract.EmployeeId}");

                try
                {
                    EmployeeContract employeeContract = await _dfoClient.GetEmployeeContractAsync(contract.EmployeeId, contract.ContractId);
                    Console.WriteLine($"Found {employeeContract}");
                    Employee employee = await _dfoClient.GetEmployee(employeeContract.Id);
                    Console.WriteLine($"Found {employee}");

                    await employeeContractHandler.RunAsync(employee, contract);
                }
                catch (Exception ex) { Console.WriteLine($"Unhandled error occured:{Environment.NewLine}{ex}"); }
            }
        }

        public async Task Archive(IEmployeeContractHandler employeeContractHandler, List<string> contractSequenceList)
        {
            await init();

            Console.WriteLine($"Prosesserer {contractSequenceList.Count} kontrakter...");
            Context.CurrentLogger.WriteToLog($"Processing {contractSequenceList.Count} contracts");

            foreach (string contractSequenceNumber in contractSequenceList)
            {
                Contract contract = await _dfoClient.GetContractAsync(contractSequenceNumber);
                Console.WriteLine($"Working on {contract.SequenceNumber};{contract.ContractId};{contract.EmployeeId}");

                try
                {
                    EmployeeContract employeeContract = await _dfoClient.GetEmployeeContractAsync(contract.EmployeeId, contract.ContractId);
                    Console.WriteLine($"Found {employeeContract}");
                    Employee employee = await _dfoClient.GetEmployee(employeeContract.Id);
                    Console.WriteLine($"Found {employee}");

                    await employeeContractHandler.RunAsync(employee, contract);
                }
                catch (Exception ex) { Console.WriteLine($"Unhandled error occured:{Environment.NewLine}{ex}"); }
            }
        }

        public async Task<List<string>> GetContractsFromDfo(DateTimeOffset from, DateTimeOffset to)
        {
            await init();
            return await _dfoClient.GetContractSequenceList(from, to);
        }

        private async Task init()
        {
            if (_dfoClient != null) return;

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
            var token = await maskinportenClient.GetAccessToken(Context.MaskinportenScope);
            Context.CurrentLogger.WriteToLog($"Token received... ({token.Token.Substring(10)}...)");
            _dfoClient = new Client(Context.DfoApiBaseAddress, token.Token);
        }
    }
}
