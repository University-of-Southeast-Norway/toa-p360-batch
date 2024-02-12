using DfoClient;

namespace DfoToa.Domain;

public interface IContext : Archive.IContext
{
    string DfoApiBaseAddress { get; }
    string MaskinportenCertificatePath { get; }
    string MaskinportenCertificatePassword { get; }
    string MaskinportenAudience { get; }
    string MaskinportenTokenEndpoint { get; }
    string MaskinportenIssuer { get; }
    string MaskinportenScope { get; }
    bool UseApiKey { get; }
    string StateFolder { get; }
    IReport Reporter { get; }
    IHandleStateFiles StateFileHandler { get; }
    DateTimeOffset SearchDate { get; }
    ITokenResolver TokenResolver { get; }
    IProvideApiKey ApiKeyProvider { get; }
}
