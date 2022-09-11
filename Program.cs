using System;
using System.IO;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using Ks.Fiks.Maskinporten.Client;
using System.Net.Http.Headers;
using System.Net.Http;
using System.Collections.Generic;
using dfo_toa_manual.DFO;

namespace dfo_toa_manual
{
    internal class Program
    {
        protected static dynamic config;
        protected static MaskinportenClient maskinportenClient;
        protected static X509Certificate2 certificate;

        static async Task Main(string[] args)
        {
            Console.Write("Angi startdato (yyyymmdd): ");
            String dateFrom = Console.ReadLine();
            Console.Write("Angi sluttdato (yyyymmdd): ");
            String dateTo = Console.ReadLine();
            Console.WriteLine("Er du sikker på at du vil arkivere kontrakter fra " + dateFrom + " til " + dateTo + "? Skriv ja for å fortsette eller trykk enter for å avslutte.");
            Boolean proceed = Console.ReadLine() == "ja" ? true : false;

            if (proceed)
            {
                Console.WriteLine("Fetching contracts from " + dateFrom + " to " + dateTo + "...");

                await Program.Archive(dateFrom, dateTo);
            }

            Console.WriteLine("Arkivering avsluttet");
            Console.ReadLine();
        }

        protected static async Task Archive(String dateFrom, String dateTo)
        {
            await Program.init().ContinueWith(t =>
            {
                try
                {
                    var token = t.Result;
                    Log.LogToFile("Token received... (" + token.Token.Substring(0, 10) + "...)");

                    // Call api...
                    HttpClient client = new HttpClient();
                    client.BaseAddress = new Uri((string)Program.config.dfo.api_base);
                    client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token.Token);

                    List<string> contractSequenceList = API.getContractSequenceList(client, dateFrom, dateTo);
                    
                    Console.WriteLine("Prosesserer " + contractSequenceList.Count + " kontrakter...");
                    Log.LogToFile("Processing " + contractSequenceList.Count + " contracts");
                    
                    foreach (string contractSequenceNumber in contractSequenceList)
                    {
                        Contract contract = API.getContract(client, contractSequenceNumber);
                        Console.WriteLine("Working on " + contract.SequenceNumber + ";" + contract.ContractId + ";" + contract.EmployeeId);
                        
                        try
                        {
                            EmployeeContract employeeContract = API.getEmployeeContract(client, contract.EmployeeId, contract.ContractId);
                            Console.WriteLine("Found " + employeeContract.ToString());
                            Employee employee = API.getEmployee(client, employeeContract.Id);
                            Console.WriteLine("Found " + employee.ToString());

                            /*
                            DocumentService.Files2 contractFile = new DocumentService.Files2();
                            contractFile.Title = contract.ContractId;
                            contractFile.Format = "pdf";
                            List<object> data = new List<object>();
                            data.Add(contract.FileContent);
                            contractFile.Data = data;

                            P360BusinessLogic.Run(employee.SocialSecurityNumber, employee.FirstName, null, employee.LastName, employee.Address, employee.Zipcode, employee.City, employee.PhoneNumber, employee.Email, contractFile);
                            */
                        }
                        catch (Exception ex) { Console.WriteLine("Data not found, skipping..."); }
                    }
                }
                catch (Exception ex) { Log.LogToFile(ex.ToString()); }
                finally { Log.Flush(); }
            });
        }

        protected async static Task<MaskinportenToken> init()
        {
            Log.LogToFile("Initializing...");
            Program.config = JObject.Parse(File.ReadAllText(@"JSON\_general.json"));

            Program.certificate = new X509Certificate2(
                (string)Program.config.maskinporten.certificate.path,
                (string)Program.config.maskinporten.certificate.password
            );

            var configuration = new MaskinportenClientConfiguration(
                audience: (string) Program.config.maskinporten.audience,
                tokenEndpoint: (string) Program.config.maskinporten.token_endpoint,
                issuer: (string) Program.config.maskinporten.issuer,
                numberOfSecondsLeftBeforeExpire: 10,
                certificate: Program.certificate);

            Program.maskinportenClient = new MaskinportenClient(configuration);

            Log.LogToFile("Get access token from Maskinporten...");
            return await Program.maskinportenClient.GetAccessToken((string)Program.config.maskinporten.scope);
        }
    }
}
