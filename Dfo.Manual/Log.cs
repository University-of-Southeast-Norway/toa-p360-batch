using Newtonsoft.Json.Linq;
using System;
using System.Diagnostics;
using System.IO;

namespace dfo_toa_manual
{
    public static class Log
    {
        private static readonly Stream _logFile = File.Create(DefaultContext.Current.LogFilePath);
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
