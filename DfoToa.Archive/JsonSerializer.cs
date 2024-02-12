using Newtonsoft.Json;

namespace DfoToa.Archive;

internal class JsonSerializer
{
    internal static string FromRunResult(RunResult runResult)
    {
        return JsonConvert.SerializeObject(runResult, new JsonSerializerSettings { TypeNameHandling = TypeNameHandling.All });
    }
}
