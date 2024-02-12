using System.Text;
using System.Security.Cryptography;
namespace DfoToa.Archive;

public static class Utility
{
    public static string CreateChecksum(string content)
    {
        using (var md5 = MD5.Create())
        {
            var buffer = Encoding.Default.GetBytes(content);
            string fileHash = BitConverter.ToString(md5.ComputeHash(buffer)).Replace("-", "");
            return fileHash;
        }
    }
}
