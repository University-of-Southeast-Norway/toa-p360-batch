using Newtonsoft.Json.Linq;
using System;
using System.Diagnostics;
using System.IO;

namespace dfo_toa_manual
{
    public static class Log
    {
        private static string _logFilePath = JObject.Parse(File.ReadAllText(@"JSON\_general.json") as dynamic).logFilePath;
        private static Stream _logFile = File.Create(_logFilePath);
        private static TextWriterTraceListener _traceListener;

        public static void LogToFile(string stringToLog)
        {
            if (Log._traceListener == null)
            {
                Log._traceListener = new TextWriterTraceListener(Log._logFile);
                Trace.Listeners.Add(Log._traceListener);
            }

            Trace.Write(DateTime.Now + " | " + stringToLog + "\n");
        }

        public static void Flush()
        {
            Trace.Flush();
        }
    }
}
