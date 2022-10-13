using DfoToa.Domain;
using Newtonsoft.Json.Linq;
using P360Client;
using P360Client.Domain;
using System;
using System.Collections.Specialized;
using System.Diagnostics;
using System.IO;

namespace DfoToa.BatchRun
{
    internal class DefaultContext : Domain.IContext, IDisposable
    {
        private static readonly string _jsonGeneral = File.ReadAllText(@"JSON\_general.json");
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

        private DefaultContext(){}
        public string P360BaseAddress => _dynamicGeneral.p360BaseAddress.ToString();
        public string P360ApiKey => _dynamicGeneral.p360ApiKey.ToString();
        public string InProductionDate => _dynamicGeneral.inProductionDate.ToString();
        public ILog CurrentLogger => _currentLogger;
        public string LogFilePath => $"{_dynamicGeneral.logFolder.ToString().Trim('/').Trim('\\')}/log_{DateTimeOffset.Now:dd.MM.yyyy-HH.mm.ss}.txt";
        public string MaskinportenCertificatePath => _dynamicGeneral.maskinporten.certificate.path.ToString();
        public string MaskinportenCertificatePassword => _dynamicGeneral.maskinporten.certificate.password.ToString();
        public string MaskinportenAudience => _dynamicGeneral.maskinporten.audience.ToString();
        public string MaskinportenTokenEndpoint => _dynamicGeneral.maskinporten.token_endpoint.ToString();
        public string MaskinportenIssuer => _dynamicGeneral.maskinporten.issuer.ToString();
        public string MaskinportenScope => _dynamicGeneral.maskinporten.scope.ToString();
        public string DfoApiBaseAddress => _dynamicGeneral.dfo.api_base.ToString();

        public string StateFolder => _dynamicGeneral.stateFolder.ToString();

        public IReport Reporter => _reporter ??= new ReportToFile(ReportFilePath);

        public static string? FromDate { get; set; }
        public static string? ToDate { get; set; }

        public string ReportFilePath => $"{_dynamicGeneral.reportFolder.ToString().Trim('/').Trim('\\')}/report_{FromDate}-{ToDate}.txt";

        public IHandleStateFiles StateFileHandler => _stateFileHandler ??= new DStepFileHandler(StateFolder);

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