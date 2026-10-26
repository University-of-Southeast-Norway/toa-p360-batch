namespace DfoToa.BatchRun;

internal record class Configuration
{
    public string? InProductionDate { get; set; }

    public string LogFolder { get; set; } = string.Empty;

    public string ReportFolder { get; set; } = string.Empty;

    public string StateFolder { get; set; } = string.Empty;

    public DfoConfiguration Dfo { get; set; } = new() { Api_Base = string.Empty };

    public DfoApiKeysConfiguration[]? Api_Keys { get; set; }

    public MaskinportenConfiguration? Maskinporten { get; set; }

    public TemplateConfiguration? Template { get; set; }

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
        public string? Password { get; set; }
    }

    public record class TemplateConfiguration
    {
        public CaseManagerConfiguration? CaseManager { get; set; }
    }

    public record class CaseManagerConfiguration
    {
        public string? ExternalId { get; set; }
        public string? Email { get; set; }
    }
}
