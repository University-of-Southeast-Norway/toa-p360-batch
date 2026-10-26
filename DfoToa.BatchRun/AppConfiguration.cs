using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Configuration.Memory;

namespace DfoToa.BatchRun;

internal static class AppConfiguration
{
    internal const string P360Section = "P360";

    // Keys from the pre-IHost _general.json format that don't bind to the current format by name.
    // Other legacy keys (logFolder, dfo:api_base, api_keys, ...) bind as-is since configuration keys are case-insensitive.
    private static readonly (string Legacy, string Current)[] _legacyKeys =
    [
        ("p360ApiKey", $"{P360Section}:ApiKey"),
        ("p360BaseAddress", $"{P360Section}:BaseAddress"),
        ("p360IntArk:caseServices", $"{P360Section}:CaseService"),
        ("p360IntArk:documentServices", $"{P360Section}:DocumentService"),
        ("p360IntArk:contactServices", $"{P360Section}:ContactService"),
        ("p360IntArk:supportServices", $"{P360Section}:SupportService"),
        ("maskinporten:token_endpoint", "Maskinporten:TokenEndpoint"),
    ];

    internal static IConfigurationRoot Build()
    {
        IConfigurationBuilder builder = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("JSON/_general.json")
            .AddEnvironmentVariables();

        // Lowest precedence, so keys in the current format win over mapped legacy keys.
        builder.Sources.Insert(0, new MemoryConfigurationSource { InitialData = MapLegacyKeys(builder.Build()) });
        return builder.Build();
    }

    internal static bool UseIntArk(IConfiguration configuration) => configuration.GetSection($"{P360Section}:CaseService").Exists();

    private static Dictionary<string, string?> MapLegacyKeys(IConfiguration configuration)
    {
        Dictionary<string, string?> mapped = new() { [$"{P360Section}:AdContextUser"] = @"swi\360integration" };
        foreach ((string legacy, string current) in _legacyKeys)
        {
            IConfigurationSection section = configuration.GetSection(legacy);
            if (section.Value is not null) mapped[current] = section.Value;
            foreach (KeyValuePair<string, string?> entry in section.AsEnumerable(makePathsRelative: true))
            {
                if (entry.Value is not null) mapped[$"{current}:{entry.Key}"] = entry.Value;
            }
        }
        return mapped;
    }
}
