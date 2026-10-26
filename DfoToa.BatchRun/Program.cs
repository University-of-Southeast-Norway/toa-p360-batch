using DfoToa.BatchRun;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using P360Client;
using P360Client.Extensions;

ArgumentUtility argumentUtility = new(Environment.GetCommandLineArgs());
if (argumentUtility.HelpNeeded()) return;

IConfigurationRoot configuration = AppConfiguration.Build();

IHostBuilder hostBuilder = Host.CreateDefaultBuilder();
hostBuilder.ConfigureAppConfiguration(builder =>
{
    builder.Sources.Clear();
    builder.AddConfiguration(configuration);
});
hostBuilder.ConfigureLogging(logging => logging.SetMinimumLevel(LogLevel.Warning));
hostBuilder.ConfigureServices((context, services) =>
{
    services.Configure<ConsoleLifetimeOptions>(options => options.SuppressStatusMessages = true);

    Configuration settings = new();
    context.Configuration.Bind(settings);
    services.AddSingleton(settings);

    services.AddSingleton(argumentUtility);
    services.AddSingleton(new ConsoleHelper(argumentUtility.Silent));
    services.AddSingleton<ILog>(_ => new FileLog($"{settings.LogFolder.Trim('/').Trim('\\')}/log_{DateTimeOffset.Now:dd.MM.yyyy-HH.mm.ss}.txt"));
    services.AddSingleton<DfoToa.Domain.IContext, DefaultContext>();
    services.AddHostedService<ArchiveJob>();
});

IP360ClientBuilder p360ClientBuilder = hostBuilder.AddP360Client();
if (AppConfiguration.UseIntArk(configuration)) p360ClientBuilder.UseP360ApiWithIntArk(AppConfiguration.P360Section);
else p360ClientBuilder.UseDefaultClientResources(AppConfiguration.P360Section);

await hostBuilder.Build().RunAsync();

Console.WriteLine("Arkivering avsluttet");
Console.ReadLine();
