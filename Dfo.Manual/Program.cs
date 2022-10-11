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
            String dateFrom = null;
            String dateTo = null;

#if DEBUG
            dateFrom = "20220910";
            dateTo = "20220911";
            Boolean proceed = true;
#else

            if (Environment.GetCommandLineArgs().Length > 1) dateFrom = Environment.GetCommandLineArgs()[1];
            if (Environment.GetCommandLineArgs().Length > 2) dateTo = Environment.GetCommandLineArgs()[2];
            else if (dateFrom != null) dateTo = DateTimeOffset.Now.Date.ToString("yyyyMMdd");
            if (dateFrom == null)
            {
                Console.Write("Angi startdato (yyyymmdd): ");
                dateFrom = Console.ReadLine();
            }
            if (dateTo == null)
            {
                Console.Write("Angi sluttdato (yyyymmdd): ");
                dateTo = Console.ReadLine();
            }
            Console.WriteLine("Er du sikker på at du vil arkivere avtaler fra " + dateFrom + " til " + dateTo + "? Skriv ja for å fortsette eller trykk enter for å avslutte.");
            DefaultContext.FromDate = dateFrom;
            DefaultContext.ToDate = dateTo;
            Boolean proceed = Console.ReadLine() == "ja" ? true : false;
#endif

            using (var context = DefaultContext.Current)
            {
                if (proceed)
                {
                    Console.WriteLine("Henter avtaler fra " + dateFrom + " til " + dateTo + "...");

                    try
                    {
                        context.CurrentLogger.WriteToLog($"Henter avtaler {dateFrom} til {dateTo}...");
                        var handler = new ArchiveHandler(context);
                        var contracts = await handler.GetContractsFromDfo(DateTimeOffset.ParseExact(dateFrom, "yyyyMMdd", CultureInfo.InvariantCulture),
                            DateTimeOffset.ParseExact(dateTo, "yyyyMMdd", CultureInfo.InvariantCulture));
                        Console.Write($"Fant {contracts.Count()} kontrakter. Ønsker du å arkivere disse? (Ja/Nei) ");
                        if (Console.ReadLine()?.ToLower()?.Contains("n") == true) return;
                        await handler.Archive(new P360EmployeeContractHandler(context), contracts);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex.ToString());
                        Log.LogToFile(ex.ToString());
                    }
                }
            }

            Console.WriteLine("Arkivering avsluttet");
            Console.ReadLine();
        }
    }
}
