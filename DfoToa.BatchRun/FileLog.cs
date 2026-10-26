using P360Client;

namespace DfoToa.BatchRun;

internal sealed class FileLog : ILog, IDisposable
{
    private readonly string _path;
    private readonly object _theLock = new();
    private StreamWriter? _writer;

    public FileLog(string path)
    {
        _path = path;
    }

    public void WriteToLog(params object[] values)
    {
        lock (_theLock)
        {
            _writer ??= new StreamWriter(File.Create(_path)) { AutoFlush = true };
            foreach (object value in values)
            {
                _writer.Write($"{DateTime.Now} | {value}\n");
            }
        }
    }

    public void Dispose()
    {
        _writer?.Dispose();
    }
}
