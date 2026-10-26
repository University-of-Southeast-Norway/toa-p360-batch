// See https://aka.ms/new-console-template for more information
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using P360Client.Extensions;


IHostBuilder hostBuilder = Host.CreateDefaultBuilder();
hostBuilder.ConfigureAppConfiguration(options => {
    options.AddJsonFile("JSON/_general.json");
    });
hostBuilder.ConfigureServices((context, services) =>
{
    Configuration configuration = new();
    context.Configuration.Bind(configuration);
    services.AddSingleton(configuration);

    services.AddSingleton<DfoToa.Domain.IContext, DefaultContext>();
});

hostBuilder.AddP360Client().UseP360ApiWithIntArk();

IHost host = hostBuilder.Build();
var c = host.Services.GetRequiredService<Configuration>();
host.Run();
