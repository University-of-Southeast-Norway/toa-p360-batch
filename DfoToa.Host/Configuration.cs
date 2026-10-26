// See https://aka.ms/new-console-template for more information
internal record class Configuration
{
    public string? InProductionDate { get; set; }

    public string LogFolder { get; set; }

    public string ReportFolder { get; set; }

    public string StateFolder { get; set; }

    public string? P360ApiKey { get; set; }

    public string? P360BaseAddress { get; set; }

    public P360Intark? P360IntArk { get; set; }

    public DfoConfiguration Dfo { get; set; }

    public DfoApiKeysConfiguration[]? Api_Keys { get; set; }

    public record class P360Intark
    {
        public P360ServicesConfiguration? CaseServices { get; set; }
        public P360ServicesConfiguration? DocumentServices { get; set; }
        public P360ServicesConfiguration? ContactServices { get; set; }
        public P360ServicesConfiguration? SupportServices { get; set; }
    }

    public record class P360ServicesConfiguration
    {
        public required string BaseAddress { get; set; }
        public required string ApiKey { get; set; }
    }

    public record class DfoConfiguration
    {
        public required string Api_Base { get; set; }
    }

    public record class DfoApiKeysConfiguration
    {
        public required string Scope { get; set; }
        public required string Header { get; set; }
        public required string Value { get; set; }
    }

    public record class MaskinportenConfiguration
    {
        public required CertificateConfiguration Certificate { get; set; }
        public required string Audience { get; set; }
        public required string TokenEndpoint { get; set; }
        public required string Issuer { get; set; }
        public required string Scope { get; set; }
    }

    public record class CertificateConfiguration
    {
        public required string Path { get; set; }
        public required string Password { get; set; }
    }

}
