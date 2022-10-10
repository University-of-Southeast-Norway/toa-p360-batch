using DfoToa.Domain;
using DfoToa.BatchRun;
using System.Globalization;


String? dateFrom = null;
String? dateTo = null;

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


if (proceed)
{
    Console.WriteLine("Henter avtaler " + dateFrom + " til " + dateTo + "...");

    using (var context = DefaultContext.Current)
    {
        try
        {
            context.CurrentLogger.WriteToLog($"Henter avtaler {dateFrom} til {dateTo}...");
            var handler = new ArchiveHandler(context);
            var contracts = await handler.GetContractsFromDfo(DateTimeOffset.ParseExact(dateFrom, "yyyyMMdd", CultureInfo.InvariantCulture),
                DateTimeOffset.ParseExact(dateTo, "yyyyMMdd", CultureInfo.InvariantCulture));
            Console.Write($"Fant {contracts.Count()} avtaler. Ønsker du å arkivere disse? (Ja/Nei) ");
            if (Console.ReadLine()?.ToLower()?.Contains("n") == true) return;
            await handler.Archive(new P360EmployeeContractHandler(context), contracts);
        }
        catch (Exception ex) { Log.LogToFile(ex.ToString()); }
    }
}

Console.WriteLine("Arkivering avsluttet");
Console.ReadLine();