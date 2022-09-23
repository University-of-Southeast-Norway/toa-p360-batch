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