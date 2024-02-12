using CaseService;
using ContactService;
using DocumentService;
using Newtonsoft.Json;
using System.Text;

namespace DfoToa.Archive;

internal class JsonDeserializerObsolete
{
    private readonly static string jsonConfigFileSynchronizePrivatePerson = File.ReadAllText(@"Definitions/synchronize_private_person.json", Encoding.UTF8);
    private readonly static string jsonConfigFileGetPrivatePersons = File.ReadAllText(@"Definitions/get_private_persons.json", Encoding.UTF8);
    private readonly static string jsonConfigFileCreateCase = File.ReadAllText(@"Definitions/create_case.json", Encoding.UTF8);
    private readonly static string jsonConfigFileGetCases = File.ReadAllText(@"Definitions/get_cases.json", Encoding.UTF8);
    private readonly static string jsonConfigFileGetCasesResult = File.ReadAllText(@"Definitions/get_cases_result.json", Encoding.UTF8);
    private readonly static string jsonConfigFileCreateDocument = File.ReadAllText(@"Definitions/create_document.json", Encoding.UTF8);
    private readonly static string jsonConfigFileUploadFile = File.ReadAllText(@"Definitions/upload_file.json", Encoding.UTF8);
    private readonly static string jsonConfigFileUpdateDocument = File.ReadAllText(@"Definitions/update_document.json", Encoding.UTF8);
    private readonly static string jsonConfigFileSignOffDocument = File.ReadAllText(@"Definitions/sign_off_document.json", Encoding.UTF8);
    private readonly static string jsonConfigFileGetDocuments = File.ReadAllText(@"Definitions/get_documents.json", Encoding.UTF8);

    internal static GetCasesArgs GetGetCaseArgs()
    {
        return JsonConvert.DeserializeObject<GetCasesArgs>(jsonConfigFileGetCases);
    }

    internal static CreateCaseArgs GetCreateCaseArgs()
    {
        return JsonConvert.DeserializeObject<CreateCaseArgs>(jsonConfigFileCreateCase);
    }

    internal static CreateDocumentArgs GetCreateDocumentArgs()
    {
        return JsonConvert.DeserializeObject<CreateDocumentArgs>(jsonConfigFileCreateDocument);
    }

    internal static GetDocumentsArgs GetGetDocumentArgs()
    {
        return JsonConvert.DeserializeObject<GetDocumentsArgs>(jsonConfigFileGetDocuments);
    }

    internal static GetPrivatePersonsArgs GetPrivatePersonsArgs()
    {
        return JsonConvert.DeserializeObject<GetPrivatePersonsArgs>(jsonConfigFileGetPrivatePersons);
    }

    internal static SynchronizePrivatePersonArgs GetSynchronizePrivatePersonArgs()
    {
        return JsonConvert.DeserializeObject<SynchronizePrivatePersonArgs>(jsonConfigFileSynchronizePrivatePerson);
    }

    internal static UpdateDocumentArgs GetUpdateDocumentArgs()
    {
        return JsonConvert.DeserializeObject<UpdateDocumentArgs>(jsonConfigFileUpdateDocument);
    }

    internal static SignOffDocumentArgs GetSignOffDocumentArgs()
    {
        return JsonConvert.DeserializeObject<SignOffDocumentArgs>(jsonConfigFileSignOffDocument);
    }

    internal static FileService.UploadArgs GetUploadArgs()
    {
        return JsonConvert.DeserializeObject<FileService.UploadArgs>(jsonConfigFileUploadFile);
    }

    internal static RunResult GetRunResult(string json)
    {
        return JsonConvert.DeserializeObject<RunResult>(json, new JsonSerializerSettings { TypeNameHandling = TypeNameHandling.All });
    }
}
