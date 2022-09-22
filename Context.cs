using Newtonsoft.Json.Linq;
using P360Client;
using P360Client.Domain;
using System;
using System.Collections.Specialized;
using System.Diagnostics;
using System.IO;

namespace dfo_toa_manual
{
    internal class DefaultContext : P360Client.Domain.IContext
    {
        private static readonly string _jsonGeneral = File.ReadAllText(@"JSON\_general.json");
        private readonly dynamic _dynamicGeneral = JObject.Parse(_jsonGeneral);
        private static DefaultContext _defaultContext;
        private static readonly object _theLock = new object();
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
        public string P360ApiKey => "?authkey=" + _dynamicGeneral.p360ApiKey.ToString();
        public string InProductionDate => _dynamicGeneral.inProductionDate.ToString();
        public ILog CurrentLogger => _currentLogger;
        public string LogFilePath => _dynamicGeneral.logFilePath.ToString();
        public string MaskinportenCertificatePath => _dynamicGeneral.maskinporten.certificate.path.ToString();
        public string MaskinportenCertificatePassword => _dynamicGeneral.maskinporten.certificate.password.ToString();
        public string MaskinportenAudience => _dynamicGeneral.maskinporten.audience.ToString();
        public string MaskinportenTokenEndpoint => _dynamicGeneral.maskinporten.token_endpoint.ToString();
        public string MaskinportenIssuer => _dynamicGeneral.maskinporten.issuer.ToString();
        public string MaskinportenScope => _dynamicGeneral.maskinporten.scope.ToString();


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
    }
}