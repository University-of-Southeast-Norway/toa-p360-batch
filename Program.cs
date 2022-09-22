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
using P360Client.Domain;

namespace dfo_toa_manual
{
    internal class Program
    {
        protected static dynamic config;
        protected static MaskinportenClient maskinportenClient;
        protected static X509Certificate2 certificate;

        static async Task Main(string[] args)
        {
#if DEBUG
            String dateFrom = "20220910";
            String dateTo = "20220911";
            Boolean proceed = true;
#else
            Console.Write("Angi startdato (yyyymmdd): ");
            String dateFrom = Console.ReadLine();
            Console.Write("Angi sluttdato (yyyymmdd): ");
            String dateTo = Console.ReadLine();
            Console.WriteLine("Er du sikker på at du vil arkivere kontrakter fra " + dateFrom + " til " + dateTo + "? Skriv ja for å fortsette eller trykk enter for å avslutte.");
            Boolean proceed = Console.ReadLine() == "ja" ? true : false;
#endif

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
            await Program.init().ContinueWith(async t =>
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


                            DocumentService.Files2 contractFile = new DocumentService.Files2();
                            contractFile.Title = contract.ContractId;
                            contractFile.Format = "pdf";
                            contractFile.Base64Data = contract.FileContent;
                            P360BusinessLogic.Context = DefaultContext.Current;
                            await P360BusinessLogic.Run(employee.SocialSecurityNumber, employee.FirstName, null, employee.LastName, employee.Address, employee.Zipcode, employee.City, employee.PhoneNumber, employee.Email, contractFile);
                        }
                        catch (Exception ex) { Console.WriteLine($"Unhandled error occured:{Environment.NewLine}{ex}"); }
                    }
                }
                catch (Exception ex) { Log.LogToFile(ex.ToString()); }
                finally { Log.Flush(); }
            }).Unwrap();
        }

        protected async static Task<MaskinportenToken> init()
        {
            Log.LogToFile("Initializing...");
            Program.config = JObject.Parse(File.ReadAllText(@"JSON\_general.json"));

            Program.certificate = new X509Certificate2(
                DefaultContext.Current.MaskinportenCertificatePath,
                DefaultContext.Current.MaskinportenCertificatePassword
            );

            var configuration = new MaskinportenClientConfiguration(
                audience: DefaultContext.Current.MaskinportenAudience,
                tokenEndpoint: DefaultContext.Current.MaskinportenTokenEndpoint,
                issuer: DefaultContext.Current.MaskinportenIssuer,
                numberOfSecondsLeftBeforeExpire: 10,
                certificate: Program.certificate);

            Program.maskinportenClient = new MaskinportenClient(configuration);

            Log.LogToFile("Get access token from Maskinporten...");
            return await Program.maskinportenClient.GetAccessToken(DefaultContext.Current.MaskinportenScope);
        }
    }
}
