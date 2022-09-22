using Newtonsoft.Json.Linq;
using P360Client;
using P360Client.Domain;
using System.Collections.Specialized;
using System.Diagnostics;
using System.IO;

namespace dfo_toa_manual
{
    internal class Context : P360Client.Domain.IContext
    {
        private static readonly string jsonGeneral = File.ReadAllText(@"JSON\_general.json");
        private readonly dynamic dynamicInProductionDate = JObject.Parse(jsonGeneral);
        public string Environment => throw new System.NotImplementedException();

        public string P360BaseAddress => dynamicInProductionDate.p360BaseAddress.ToString();

        public string P360ApiKey => "?authkey=" + dynamicInProductionDate.p360ApiKey.ToString();

        public string InProductionDate => dynamicInProductionDate.inProductionDate.ToString();

        public ILog CurrentLogger => _currentLogger;
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