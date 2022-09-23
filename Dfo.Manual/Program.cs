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
using System.Runtime.Remoting.Contexts;
using DfoToa.Domain;
using System.Globalization;

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

            if (proceed)
            {
                Console.WriteLine("Fetching contracts from " + dateFrom + " to " + dateTo + "...");

                try
                {
                    await new ArchiveHandler(DefaultContext.Current).Archive(new P360EmployeeContractHandler(DefaultContext.Current),
                        DateTimeOffset.ParseExact(dateFrom, "yyyyMMdd", CultureInfo.InvariantCulture),
                        DateTimeOffset.ParseExact(dateTo, "yyyyMMdd", CultureInfo.InvariantCulture));
                }
                catch (Exception ex) { Log.LogToFile(ex.ToString()); }
                finally { Log.Flush(); }
            }

            Console.WriteLine("Arkivering avsluttet");
            Console.ReadLine();
        }
    }
}
