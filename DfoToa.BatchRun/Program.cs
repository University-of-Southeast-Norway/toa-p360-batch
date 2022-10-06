using DfoToa.Domain;
using DfoToa.BatchRun;
using System.Globalization;

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

    using (var context = DefaultContext.Current)
    {
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