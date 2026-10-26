using DfoClient;
using DfoToa.Domain;
using Ks.Fiks.Maskinporten.Client;
using Microsoft.Extensions.Options;
using P360Client;
using P360Client.Configurations;
using P360Client.Resources;
using System.Globalization;
using System.Security.Cryptography.X509Certificates;
using System.Text.Json;

namespace DfoToa.BatchRun;

internal class DefaultContext : Domain.IContext, IDisposable
{
    private readonly Configuration _configuration;
    private readonly ArgumentUtility _arguments;
    private readonly ClientOptions _clientOptions;
    private IReport? _reporter;
    private IHandleStateFiles? _stateFileHandler;
    private IProvideApiKey? _apiKeyProvider;
    private IMappingTemplates? _mappingTemplates;

    public DefaultContext(
        Configuration configuration,
        ArgumentUtility arguments,
        ILog currentLogger,
        IOptions<ClientOptions> clientOptions,
        ICaseResources caseResources,
        IDocumentResources documentResources,
        IContactResources contactResources,
        ISupportResources supportResources)
    {
        _configuration = configuration;
        _arguments = arguments;
        _clientOptions = clientOptions.Value;
        CurrentLogger = currentLogger;
        CaseResources = caseResources;
        DocumentResources = documentResources;
        ContactResources = contactResources;
        SupportResources = supportResources;
        TokenResolver = new TokenResolverClass(this);
    }

    public string BaseAddress => _clientOptions.BaseAddress;
    public string ApiKey => _clientOptions.ApiKey;
    public string AdContextUser => _clientOptions.AdContextUser;

    public ICaseResources CaseResources { get; }
    public IDocumentResources DocumentResources { get; }
    public IContactResources ContactResources { get; }
    public ISupportResources SupportResources { get; }

    public string InProductionDate => _configuration.InProductionDate!;
    public ILog CurrentLogger { get; }
    public string MaskinportenCertificatePath => _configuration.Maskinporten?.Certificate.Path ?? string.Empty;
    public string MaskinportenCertificatePassword => _configuration.Maskinporten?.Certificate.Password ?? string.Empty;
    public string MaskinportenAudience => _configuration.Maskinporten?.Audience ?? string.Empty;
    public string MaskinportenTokenEndpoint => _configuration.Maskinporten?.TokenEndpoint ?? string.Empty;
    public string MaskinportenIssuer => _configuration.Maskinporten?.Issuer ?? string.Empty;
    public string MaskinportenScope => _configuration.Maskinporten?.Scope ?? string.Empty;
    public bool UseApiKey => _configuration.Api_Keys != null;
    public string DfoApiBaseAddress => _configuration.Dfo.Api_Base;

    public string StateFolder => _configuration.StateFolder;

    public IReport Reporter => _reporter ??= new ReportToFile(ReportFilePath);

    public string ReportFilePath => $"{_configuration.ReportFolder.Trim('/').Trim('\\')}/report_{_arguments.FromDate}-{_arguments.ToDate}.txt";

    public IHandleStateFiles StateFileHandler => _stateFileHandler ??= new DStepFileHandler(StateFolder);

    public DateTimeOffset SearchDate => DateTimeOffset.ParseExact(InProductionDate, "yyyy-MM-dd", CultureInfo.InvariantCulture);

    public string? CaseManagerExternalId => _configuration.Template?.CaseManager?.ExternalId;
    public string? CaseManagerEmail => _configuration.Template?.CaseManager?.Email;

    public ITokenResolver TokenResolver { get; }

    public IProvideApiKey ApiKeyProvider => _apiKeyProvider ??= GetApiKeyProvider();

    public IMappingTemplates MappingTemplates => _mappingTemplates ??= new FromJsonMappingTemplate();

    private class FromJsonMappingTemplate : IMappingTemplates
    {
        private static readonly string _jsonTemplates = File.ReadAllText(@"JSON/templates.json");
        private readonly Templates? _templates;
        internal FromJsonMappingTemplate()
        {
            _templates = JsonSerializer.Deserialize<Templates>(_jsonTemplates, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });
        }
        public Task<UniqueQueryAttributesTemplate?> GetResponsibleTemplate()
        {
            return Task.FromResult(_templates!.Responsible);
        }
        public Task<UniqueTitlesTemplate?> GetTitlesTemplate()
        {
            return Task.FromResult(_templates!.Titles);
        }

        private class Templates
        {
            public UniqueQueryAttributesTemplate? Sender { get; set; }
            public UniqueQueryAttributesTemplate? Responsible { get; set; }
            public UniqueQueryAttributesTemplate? Receiver { get; set; }
            public UniqueTitlesTemplate? Titles { get; set; }
        }
    }

    private IProvideApiKey GetApiKeyProvider()
    {
        ApiKeyListBuilder builder = new();
        foreach (Configuration.DfoApiKeysConfiguration apiKey in _configuration.Api_Keys!)
        {
            builder.WithScope(apiKey.Scope, apiKey.Header, apiKey.Value);
        }
        return builder;
    }

    private class TokenResolverClass : ITokenResolver
    {
        private readonly Domain.IContext _context;
        private static SemaphoreSlim _theLock = new SemaphoreSlim(1, 1);

        public TokenResolverClass(Domain.IContext context)
        {
            _context = context;
        }

        private Dictionary<string, MaskinportenToken> _maskinportenTokens = new Dictionary<string, MaskinportenToken>();

        public async Task<string> GetTokenAsync(string scope)
        {
            await _theLock.WaitAsync();
            try
            {
                if (_maskinportenTokens.ContainsKey(scope) && _maskinportenTokens[scope].IsExpiring() == false)
                {
                    return _maskinportenTokens[scope].Token;
                }

                X509Certificate2 certificate = X509CertificateLoader.LoadPkcs12FromFile(
                    _context.MaskinportenCertificatePath,
                    _context.MaskinportenCertificatePassword
                );

                var configuration = new MaskinportenClientConfiguration(
                    audience: _context.MaskinportenAudience,
                    tokenEndpoint: _context.MaskinportenTokenEndpoint,
                    issuer: _context.MaskinportenIssuer,
                    numberOfSecondsLeftBeforeExpire: 10,
                    certificate: certificate);
                var maskinportenClient = new MaskinportenClient(configuration);

                MaskinportenToken tokenResult = await maskinportenClient.GetAccessToken(scope);
                if (_maskinportenTokens.ContainsKey(scope)) _maskinportenTokens[scope] = tokenResult;
                else _maskinportenTokens.Add(scope, tokenResult);
                return tokenResult.Token;
            }
            finally
            {
                _theLock.Release();
            }
        }
    }

    public void Dispose()
    {
        if (_reporter != null && _reporter is IDisposable disposableReporter)
        {
            disposableReporter.Dispose();
            _reporter = null;
        }
        if (_stateFileHandler != null && _stateFileHandler is IDisposable disposableStateFileHandler)
        {
            disposableStateFileHandler.Dispose();
            _stateFileHandler = null;
        }
    }
}
