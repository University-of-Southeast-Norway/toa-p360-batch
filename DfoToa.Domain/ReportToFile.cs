using System.Text;

namespace DfoToa.Domain;

public class ReportToFile : IReport, IDisposable
{
    private readonly string _reportFile;
    private readonly FileStream _stream;

    public ReportToFile(string reportFile)
    {
        _reportFile = reportFile;
        _stream = File.OpenWrite(_reportFile);
    }

    public async Task Report(string content)
    {
        byte[] contentBytes = Encoding.Default.GetBytes($"{content}{Environment.NewLine}");
        await _stream.WriteAsync(contentBytes, 0, contentBytes.Length);
        await _stream.FlushAsync();
    }

    public void Dispose()
    {
        _stream.Dispose();
    }
}
