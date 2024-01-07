using DfoClient;
using System.Security.Cryptography;
using System.Text;

namespace DfoToa.Domain;

public class DStepFileHandler : IHandleStateFiles
{
    public const string FileEnding = "dstep";

    private readonly string _stateFolder;

    public DStepFileHandler(string stateFolder)
    {
        _stateFolder = stateFolder;
    }

    private string CreateFileName(Contract contract)
    {
        using (var md5 = MD5.Create())
        {
            var buffer = Encoding.Default.GetBytes(contract.FileContent);
            string fileHash = BitConverter.ToString(md5.ComputeHash(buffer)).Replace("-", "");
            return $"{contract.SequenceNumber}-{fileHash}.{FileEnding}";
        }
    }

    private string CreatePath(Contract contract)
    {
        var fileName = CreateFileName(contract);
        string path = $"{_stateFolder.Trim('/').Trim('\\')}/{fileName}";
        return path;
    }

    public async Task<string> GetState(Contract contract)
    {
        string path = CreatePath(contract);
        if (File.Exists(path))
        {
            return await Task.FromResult(File.ReadAllText(path));
        }
        return null;
    }

    public async Task SaveState(Contract contract, string state)
    {
        string path = CreatePath(contract);
        if (!Directory.Exists(_stateFolder)) await Task.Run(() => Directory.CreateDirectory(_stateFolder));
        await Task.Run(() => File.WriteAllText(path, state));
    }
}
