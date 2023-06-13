using DfoClient;
using DfoToa.Domain;
using Ks.Fiks.Maskinporten.Client;
using Newtonsoft.Json.Linq;
using P360Client;
using P360Client.Domain;
using System;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Security.Cryptography.X509Certificates;

namespace DfoToa.BatchRun
{
    internal class DefaultContext : Domain.IContext, IDisposable
    {
        private static readonly string _jsonGeneral = File.ReadAllText(@"JSON/_general.json");
        private readonly dynamic _dynamicGeneral = JObject.Parse(_jsonGeneral);
        private static DefaultContext _defaultContext;
        private static readonly object _theLock = new object();
        private IReport? _reporter;
        private IHandleStateFiles? _stateFileHandler;
        public static DefaultContext Current { get { return GetSingleton() ; } }

        private static DefaultContext GetSingleton()
        {
            lock (_theLock)
            {
                _defaultContext = new DefaultContext() ;
                if (_defaultContext == null)
                {
                    _defaultContext = new DefaultContext();
                }
                return _defaultContext;
            }
        }

        private DefaultContext(){ _tokenResolver = new TokenResolverClass(this); }

        public string BaseAddress => _dynamicGeneral.p360BaseAddress.ToString();

        public string ApiKey => _dynamicGeneral.p360ApiKey.ToString();

        public string AdContextUser => throw new NotImplementedException();
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

        private readonly ITokenResolver _tokenResolver;
        public ITokenResolver TokenResolver => _tokenResolver;

        IProvideApiKey _apiKeyProvider;
        public IProvideApiKey ApiKeyProvider => _apiKeyProvider ?? (_apiKeyProvider = GetApiKeyProvider());

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
}