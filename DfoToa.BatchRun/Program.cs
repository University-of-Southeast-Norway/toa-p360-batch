using DfoToa.Domain;
using DfoToa.BatchRun;
using System.Globalization;


String? dateFrom = null;
String? dateTo = null;

#if RELEASE
dateFrom = "20220910";
dateTo = "20220911";
Boolean proceed = true;
#else
var argumentUtility = new ArgumentUtility(Environment.GetCommandLineArgs());
if (argumentUtility.HelpNeeded()) return;

var consoleHelper = new ConsoleHelper(argumentUtility.Silent);

dateFrom = argumentUtility.FromDate;
dateTo = argumentUtility.ToDate;
if (dateFrom == null)
{
    dateFrom = consoleHelper.GetFromDate();
}
if (dateTo == null)
{
    dateTo = consoleHelper.GetToDate();
}
DefaultContext.FromDate = dateFrom;
DefaultContext.ToDate = dateTo;
bool proceed = consoleHelper.Proceed("Er du sikker på at du vil arkivere avtaler fra " + dateFrom + " til " + dateTo + "? (Ja/Nei) ");
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
            if (!consoleHelper.Proceed($"Fant {contracts.Count()} avtaler. Ønsker du å arkivere disse? (Ja/Nei) ")) return;
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