using System;
using System.IO;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using Ks.Fiks.Maskinporten.Client;
using System.Net.Http.Headers;
using System.Net.Http;
using System.Collections.Generic;
using P360Client.Domain;
using System.Runtime.Remoting.Contexts;
using DfoToa.Domain;
using System.Globalization;
using System.Linq;

namespace dfo_toa_manual
{
    internal class Program
    {
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

            using (var context = DefaultContext.Current)
            {
                if (proceed)
                {
                    Console.WriteLine("Fetching contracts from " + dateFrom + " to " + dateTo + "...");

                    try
                    {
                        var handler = new ArchiveHandler(context);
                        var contracts = await handler.GetContractsFromDfo(DateTimeOffset.ParseExact(dateFrom, "yyyyMMdd", CultureInfo.InvariantCulture),
                            DateTimeOffset.ParseExact(dateTo, "yyyyMMdd", CultureInfo.InvariantCulture));
                        Console.Write($"Fant {contracts.Count()} kontrakter. Ønsker du å arkivere disse? (Ja/Nei) ");
                        if (Console.ReadLine()?.ToLower()?.Contains("n") == true) return;
                        await handler.Archive(new P360EmployeeContractHandler(context), contracts);
                    }
                    catch (Exception ex) { Log.LogToFile(ex.ToString()); }
                }
            }

            Console.WriteLine("Arkivering avsluttet");
            Console.ReadLine();
        }
    }
}
