using DfoToa.Domain;
using DfoToa.BatchRun;
using System.Globalization;


string? dateFrom = null;
string? dateTo = null;

var argumentUtility = new ArgumentUtility(Environment.GetCommandLineArgs());
var consoleHelper = new ConsoleHelper(argumentUtility.Silent);

if (argumentUtility.HelpNeeded()) return;

bool proceed;
if (string.IsNullOrEmpty(argumentUtility.SequenceNumber))
{
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
    proceed = consoleHelper.Proceed($"Er du sikker på at du vil arkivere avtaler fra {dateFrom} til {dateTo}? (Ja/Nei) ");
}
else proceed = consoleHelper.Proceed($"Er du sikker på at du vil arkivere avtale med sekvensnummer {argumentUtility.SequenceNumber}? (Ja/Nei) ");



if (proceed)
{

    using var context = DefaultContext.Current;
    try
    {
        var handler = new ArchiveHandler(context);
        if (string.IsNullOrEmpty(argumentUtility.SequenceNumber))
        {
            Console.WriteLine($"Henter avtaler {dateFrom} til {dateTo}...");
            context.CurrentLogger.WriteToLog($"Henter avtaler {dateFrom} til {dateTo}...");
            var contracts = await handler.GetContractsFromDfo(DateTimeOffset.ParseExact(dateFrom, "yyyyMMdd", CultureInfo.InvariantCulture),
                DateTimeOffset.ParseExact(dateTo, "yyyyMMdd", CultureInfo.InvariantCulture));
            if (!consoleHelper.Proceed($"Fant {contracts.Count} avtaler. Ønsker du å arkivere disse? (Ja/Nei) ")) return;
            await handler.Archive(new P360EmployeeContractHandler(context), contracts);
        }
        else
        {
            await handler.Archive(new P360EmployeeContractHandler(context), argumentUtility.SequenceNumber);
        }
    }
    catch (Exception ex)
    {
        Console.WriteLine(ex.ToString());
        Log.LogToFile(ex.ToString());
    }
}

Console.WriteLine("Arkivering avsluttet");
Console.ReadLine();