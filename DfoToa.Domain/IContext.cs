using System.Security;

namespace DfoClient.Domain
{
    public interface IContext : P360Client.Domain.IContext
    {
        string DfoApiBaseAddress { get; }
        string MaskinportenCertificatePath { get; }
        string MaskinportenCertificatePassword { get; }
        string MaskinportenAudience { get; }
        string MaskinportenTokenEndpoint { get; }
        string MaskinportenIssuer { get; }
        string MaskinportenScope { get; }
    }
}
