using DfoClient;
using DfoToa.Domain;
using Ks.Fiks.Maskinporten.Client;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Newtonsoft.Json.Linq;
using P360Client;
using P360Client.Configurations;
using P360Client.Resources;
using Refit;
using Service;
using SupportService;
using System.Globalization;
using System.Net;
using System.Security.Cryptography.X509Certificates;
using System.Text.Json;

namespace DfoToa.BatchRun
{
    internal class DefaultContext : Domain.IContext, IDisposable
    {
        private static readonly string _jsonGeneral = File.ReadAllText(@"JSON/_general.json");
        private readonly dynamic _dynamicGeneral;
        private static DefaultContext _currentContext = new();
        private static readonly object _theLock = new object();
        private IReport? _reporter;
        private IHandleStateFiles? _stateFileHandler;
        public static DefaultContext Current { get { return _currentContext; } }

        private DefaultContext()
        {
            _dynamicGeneral = JObject.Parse(_jsonGeneral);
            _tokenResolver = new TokenResolverClass(this);
            CaseOptions = CreateOptions(_dynamicGeneral.p360IntArk?.caseServices);
            DocumentOptions = CreateOptions(_dynamicGeneral.p360IntArk?.documentServices);
            ContactOptions = CreateOptions(_dynamicGeneral.p360IntArk?.contactServices);
            SupportOptions = CreateOptions(_dynamicGeneral.p360IntArk?.supportServices);

            IClientFactory clientFactory = new P360IntArkClientFactory(this);
            CaseResources = IsCaseServicesSetupWithIntArk ? new CaseResources(clientFactory, CaseOptions) : new CaseResources(CaseOptions);
            DocumentResources = IsDocumentServicesSetupWithIntArk ? new DocumentResources(clientFactory, DocumentOptions) : new DocumentResources(DocumentOptions);
            ContactResources = IsContactServicesSetupWithIntArk ? new ContactResources(clientFactory, ContactOptions) : new ContactResources(ContactOptions);
            SupportResources = IsContactServicesSetupWithIntArk ? new SupportResources(new SupportApiClientIntArk(SupportOptions)) : new SupportResources(new SupportApiClientDirect(ContactOptions));
        }

        public string BaseAddress => _dynamicGeneral.p360BaseAddress.ToString();
        public string ApiKey => _dynamicGeneral.p360ApiKey.ToString();

        public bool IsCaseServicesSetupWithIntArk => _dynamicGeneral.p360IntArk?.caseServices is not null;
        public bool IsDocumentServicesSetupWithIntArk => _dynamicGeneral.p360IntArk?.documentServices is not null;
        public bool IsContactServicesSetupWithIntArk => _dynamicGeneral.p360IntArk?.contactServices is not null;

        internal IOptionsMonitor<ClientOptions> CaseOptions { get; private set; }
        internal IOptionsMonitor<ClientOptions> DocumentOptions { get; private set; }
        internal IOptionsMonitor<ClientOptions> ContactOptions { get; private set; }
        internal IOptionsMonitor<ClientOptions> SupportOptions { get; private set; }

        public ICaseResources CaseResources { get; private set; }
        public IDocumentResources DocumentResources { get; private set; }
        public IContactResources ContactResources { get; private set; }
        public ISupportResources SupportResources { get; private set; }

        private Options<ClientOptions> CreateOptions(object sectionObject)
        {
            dynamic section = sectionObject;
            ClientOptions options;
            if (section is null)
            {
                options = new()
                {
                    AdContextUser = AdContextUser,
                    ApiKey = ApiKey,
                    BaseAddress = BaseAddress
                };
            }
            else
            {
                options = new()
                {
                    AdContextUser = AdContextUser,
                    ApiKey = section.apiKey,
                    BaseAddress = section.baseAddress
                };
            }
            return new Options<ClientOptions>(options);
        }

        public string AdContextUser => @"swi\360integration";
        public string InProductionDate => _dynamicGeneral.inProductionDate.ToString();
        public ILog CurrentLogger => _currentLogger;
        public string LogFilePath => $"{_dynamicGeneral.logFolder.ToString().Trim('/').Trim('\\')}/log_{DateTimeOffset.Now:dd.MM.yyyy-HH.mm.ss}.txt";
        public string MaskinportenCertificatePath => _dynamicGeneral.maskinporten?.certificate.path.ToString() ?? string.Empty;
        public string MaskinportenCertificatePassword => _dynamicGeneral.maskinporten?.certificate.password?.ToString() ?? string.Empty;
        public string MaskinportenAudience => _dynamicGeneral.maskinporten?.audience.ToString() ?? string.Empty;
        public string MaskinportenTokenEndpoint => _dynamicGeneral.maskinporten?.token_endpoint.ToString() ?? string.Empty;
        public string MaskinportenIssuer => _dynamicGeneral.maskinporten?.issuer.ToString() ?? string.Empty;
        public string MaskinportenScope => _dynamicGeneral.maskinporten?.scope.ToString() ?? string.Empty;
        public bool UseApiKey => _dynamicGeneral.api_keys != null;
        public string DfoApiBaseAddress => _dynamicGeneral.dfo.api_base.ToString();

        public string StateFolder => _dynamicGeneral.stateFolder.ToString();

        public IReport Reporter => _reporter ??= new ReportToFile(ReportFilePath);

        public static string? FromDate { get; set; }
        public static string? ToDate { get; set; }

        public string ReportFilePath => $"{_dynamicGeneral.reportFolder.ToString().Trim('/').Trim('\\')}/report_{FromDate}-{ToDate}.txt";

        public IHandleStateFiles StateFileHandler => _stateFileHandler ??= new DStepFileHandler(StateFolder);

        public DateTimeOffset SearchDate => DateTimeOffset.ParseExact(InProductionDate, "yyyy-MM-dd", CultureInfo.InvariantCulture);

        public string? CaseManagerExternalId => _dynamicGeneral.template?.caseManager?.externalId?.ToString();
        public string? CaseManagerEmail => _dynamicGeneral.template?.caseManager?.email?.ToString();

        private readonly ITokenResolver _tokenResolver;

        public ITokenResolver TokenResolver => _tokenResolver;

        IProvideApiKey _apiKeyProvider;
        public IProvideApiKey ApiKeyProvider => _apiKeyProvider ?? (_apiKeyProvider = GetApiKeyProvider());

        IMappingTemplates _mappingTemplates;
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
            var builder = new ApiKeyListBuilder();
            foreach(var apiKey in _dynamicGeneral.api_keys)
            {
                builder.WithScope(apiKey.scope.ToString(), apiKey.header.ToString(), apiKey.value.ToString());
            }
            return builder;
        }

        private ILog _currentLogger = new FileLogger();

        public class FileLogger : ILog
        {
            public void WriteToLog(params object[] values)
            {
                foreach(object value in values)
                {
                    Log.LogToFile(value.ToString());
                }
            }
        }

        public class Options<T>(T _configuration) : IOptionsMonitor<T>, IDisposable
        where T : class
        {
            public T CurrentValue => _configuration;

            public void Dispose()
            {
            }

            public T Get(string name)
            {
                return _configuration;
            }

            public IDisposable OnChange(Action<T, string> listener)
            {
                listener(_configuration, "");
                return this;
            }
        }

        public class P360IntArkClientFactory(DefaultContext _defaultContext) : IClientFactory
        {
            public TClient CreateClient<TClient>()
            {
                Type tClientType = typeof(TClient);
                return (TClient)CreateClient(tClientType);
            }

            public object? CreateClient(Type clientType)
            {
                if (clientType == typeof(CaseService.ICaseServiceClient))
                {
                    return new CaseService.Client(CreateHttpClientForIntArk(_defaultContext.CaseOptions!), httpClientConfiguredWithBaseUrl: true);
                }
                if (clientType == typeof(DocumentService.IDocumentServiceClient))
                {
                    return new DocumentService.Client(CreateHttpClientForIntArk(_defaultContext.DocumentOptions!), httpClientConfiguredWithBaseUrl: true);
                }
                if (clientType == typeof(ContactService.IContactServiceClient))
                {
                    return new ContactService.Client(CreateHttpClientForIntArk(_defaultContext.ContactOptions!), httpClientConfiguredWithBaseUrl: true);
                }

                throw new Exception("Type is invalid");
            }
            public static HttpClient CreateHttpClientForIntArk(IOptionsMonitor<ClientOptions> options)
            {
                HttpClientHandler messageHandler = new()
                {
                     AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate
                };
                HttpClient client = new(messageHandler)
                {
                    Timeout = new TimeSpan(0, 3, 0),
                    BaseAddress = new(options.CurrentValue.BaseAddress)
                };
                client.DefaultRequestHeaders.Add("X-Gravitee-Api-Key", options.CurrentValue.ApiKey);
                return client;
            }
        }

        internal class SupportApiClientDirect : ISupportServiceApiClient
        {
            private readonly AuthKeyQuery _authKeyQuery;
            private readonly ISupportServiceDirectApi _service;

            public SupportApiClientDirect(IOptionsMonitor<ClientOptions> options)
            {
                _authKeyQuery = new() { AuthKey = options.CurrentValue.ApiKey };
                HttpClient httpClient = CreateHttpClientForIntArk(options);
                _service = RestService.For<ISupportServiceDirectApi>(httpClient);
            }
            public static HttpClient CreateHttpClientForIntArk(IOptionsMonitor<ClientOptions> options)
            {
                HttpClientHandler messageHandler = new()
                {
                    AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate
                };
                HttpClient client = new(messageHandler)
                {
                    Timeout = new TimeSpan(0, 3, 0),
                    BaseAddress = new(options.CurrentValue.BaseAddress)
                };
                return client;
            }

            Task<ApiResponse<string>> ISupportServiceApiClient.Ping()
            {
                
                return _service.Ping(_authKeyQuery);
            }

            Task<ApiResponse<P360Client.DTO.GetCodeTableRowsResponse>> ISupportServiceApiClient.GetCodeTableRows(P360Client.DTO.GetCodeTableRowsArgs args)
            {
                return _service.GetCodeTableRows(_authKeyQuery, args);
            }
        }

        internal class SupportApiClientIntArk : ISupportServiceApiClient
        {
            private readonly ISupportServiceIntArkApi _service;

            public SupportApiClientIntArk(IOptionsMonitor<ClientOptions> options)
            {
                HttpClient httpClient = CreateHttpClientForIntArk(options);
                _service = RestService.For<ISupportServiceIntArkApi>(httpClient);
            }
            public static HttpClient CreateHttpClientForIntArk(IOptionsMonitor<ClientOptions> options)
            {
                HttpClientHandler messageHandler = new()
                {
                    AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate
                };
                HttpClient client = new(messageHandler)
                {
                    Timeout = new TimeSpan(0, 3, 0),
                    BaseAddress = new(options.CurrentValue.BaseAddress)
                };
                client.DefaultRequestHeaders.Add("X-Gravitee-Api-Key", options.CurrentValue.ApiKey);
                return client;
            }

            Task<ApiResponse<string>> ISupportServiceApiClient.Ping()
            {
                return _service.Ping();
            }

            Task<ApiResponse<P360Client.DTO.GetCodeTableRowsResponse>> ISupportServiceApiClient.GetCodeTableRows(P360Client.DTO.GetCodeTableRowsArgs args)
            {
                return _service.GetCodeTableRows(args);
            }
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

                    var certificate = new X509Certificate2(
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
            Log.Flush();
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

    internal interface ISupportServiceIntArkApi
    {
        [Post("/Ping")]
        Task<ApiResponse<string>> Ping();

        [Post("/GetCodeTableRows")]
        Task<ApiResponse<P360Client.DTO.GetCodeTableRowsResponse>> GetCodeTableRows([Body] P360Client.DTO.GetCodeTableRowsArgs args);
    }
    internal interface ISupportServiceDirectApi
    {
        [Post("/Biz/v2/api/call/SI.Data.RPC/SI.Data.RPC/SupportService/Ping")]
        Task<ApiResponse<string>> Ping([Query] AuthKeyQuery authKeyQuery);

        [Post("/Biz/v2/api/call/SI.Data.RPC/SI.Data.RPC/SupportService/GetCodeTableRows")]
        Task<ApiResponse<P360Client.DTO.GetCodeTableRowsResponse>> GetCodeTableRows([Query] AuthKeyQuery authKeyQuery, [Body] P360Client.DTO.GetCodeTableRowsArgs args);
    }
}