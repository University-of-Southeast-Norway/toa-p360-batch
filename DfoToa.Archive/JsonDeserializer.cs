using DfoToa.Archive;
using Newtonsoft.Json;
using P360Client.DTO;

namespace P360Client.Domain;

internal class JsonDeserializer
{
    private static readonly JsonTemplateRepository jsonTemplateRepository = new JsonTemplateRepository();

    internal static Task<GetCasesArgs> GetGetCaseArgsAsync() => jsonTemplateRepository.GetGetCaseArgsAsync();

    internal static Task<CreateCaseArgs> GetCreateCaseArgsAsync() => jsonTemplateRepository.GetCreateCaseArgsAsync();

    internal static Task<CreateDocumentArgs> GetCreateDocumentArgsAsync() => jsonTemplateRepository.GetCreateDocumentArgsAsync();

    internal static Task<GetDocumentsArgs> GetGetDocumentArgsAsync() => jsonTemplateRepository.GetGetDocumentArgsAsync();

    internal static Task<GetPrivatePersonsArgs> GetPrivatePersonsArgsAsync() => jsonTemplateRepository.GetPrivatePersonsArgsAsync();

    internal static Task<GetContactPersonsArgs> GetContactPersonsArgsAsync() => jsonTemplateRepository.GetGetContactPersonsArgsAsync();

    internal static Task<SynchronizePrivatePersonArgs> GetSynchronizePrivatePersonArgsAsync() => jsonTemplateRepository.GetSynchronizePrivatePersonArgsAsync();

    internal static Task<SynchronizeContactPersonArgs> GetSynchronizeContactPersonArgsAsync() => jsonTemplateRepository.GetSynchronizeContactPersonArgsAsync();

    internal static Task<UpdateDocumentArgs> GetUpdateDocumentArgsAsync() => jsonTemplateRepository.GetUpdateDocumentArgsAsync();

    internal static Task<SignOffDocumentArgs> GetSignOffDocumentArgsAsync() => jsonTemplateRepository.GetSignOffDocumentArgsAsync();

    internal static RunResult GetRunResult(string json)
    {
        return JsonConvert.DeserializeObject<RunResult>(json, new JsonSerializerSettings { TypeNameHandling = TypeNameHandling.All });
    }
    internal class ParameterTemplate<TParameter>
    {
        public TParameter Parameter { get; set; }
    }
}
