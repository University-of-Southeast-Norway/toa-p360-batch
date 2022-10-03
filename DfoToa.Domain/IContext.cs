using System.Security;

namespace DfoToa.Domain
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
        string StateFolder { get; }
        IReport Reporter { get; }
        IHandleStateFiles StateFileHandler { get; }
    }
}
