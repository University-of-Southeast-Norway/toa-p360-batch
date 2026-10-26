using DfoToa.Domain;
using Microsoft.Extensions.Hosting;
using System.Globalization;

namespace DfoToa.BatchRun;

internal class ArchiveJob : BackgroundService
{
    private readonly Domain.IContext _context;
    private readonly ArgumentUtility _arguments;
    private readonly ConsoleHelper _consoleHelper;
    private readonly IHostApplicationLifetime _lifetime;

    public ArchiveJob(Domain.IContext context, ArgumentUtility arguments, ConsoleHelper consoleHelper, IHostApplicationLifetime lifetime)
    {
        _context = context;
        _arguments = arguments;
        _consoleHelper = consoleHelper;
        _lifetime = lifetime;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await Task.Yield();
        try
        {
            await RunAsync();
        }
        finally
        {
            _lifetime.StopApplication();
        }
    }

    private async Task RunAsync()
    {
        bool proceed;
        if (string.IsNullOrEmpty(_arguments.SequenceNumber))
        {
            _arguments.FromDate ??= _consoleHelper.GetFromDate();
            _arguments.ToDate ??= _consoleHelper.GetToDate();
            proceed = _consoleHelper.Proceed($"Er du sikker på at du vil arkivere avtaler fra {_arguments.FromDate} til {_arguments.ToDate}? (Ja/Nei) ");
        }
        else proceed = _consoleHelper.Proceed($"Er du sikker på at du vil arkivere avtale med sekvensnummer {_arguments.SequenceNumber}? (Ja/Nei) ");

        if (!proceed) return;

        try
        {
            ArchiveHandler handler = new(_context);
            if (string.IsNullOrEmpty(_arguments.SequenceNumber))
            {
                Console.WriteLine($"Henter avtaler {_arguments.FromDate} til {_arguments.ToDate}...");
                _context.CurrentLogger.WriteToLog($"Henter avtaler {_arguments.FromDate} til {_arguments.ToDate}...");
                List<string> contracts = await handler.GetContractsFromDfo(DateTimeOffset.ParseExact(_arguments.FromDate!, "yyyyMMdd", CultureInfo.InvariantCulture),
                    DateTimeOffset.ParseExact(_arguments.ToDate!, "yyyyMMdd", CultureInfo.InvariantCulture));
                if (!_consoleHelper.Proceed($"Fant {contracts.Count} avtaler. Ønsker du å arkivere disse? (Ja/Nei) ")) return;
                await handler.Archive(new P360EmployeeContractHandler(_context), contracts);
            }
            else
            {
                await handler.Archive(new P360EmployeeContractHandler(_context), _arguments.SequenceNumber);
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.ToString());
            _context.CurrentLogger.WriteToLog(ex.ToString());
        }
    }
}
