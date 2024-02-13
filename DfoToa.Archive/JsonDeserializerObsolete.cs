using Newtonsoft.Json;
using P360Client.DTO;
using System.Text;
using File = System.IO.File;

namespace DfoToa.Archive;

internal class JsonDeserializerObsolete
{
    private readonly static string jsonConfigFileSynchronizePrivatePerson = File.ReadAllText(@"Definitions/synchronize_private_person.json", Encoding.UTF8);
    private readonly static string jsonConfigFileGetPrivatePersons = File.ReadAllText(@"Definitions/get_private_persons.json", Encoding.UTF8);
    private readonly static string jsonConfigFileGetContactPersons = File.ReadAllText(@"Definitions/get_contact_persons.json", Encoding.UTF8);
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
        return Convert<CaseService.GetCasesArgs, GetCasesArgs>(jsonConfigFileGetCases);
    }

    internal static CreateCaseArgs GetCreateCaseArgs()
    {
        return Convert<CaseService.CreateCaseArgs, CreateCaseArgs>(jsonConfigFileCreateCase);
    }

    internal static CreateDocumentArgs GetCreateDocumentArgs()
    {
        return Convert<DocumentService.CreateDocumentArgs, CreateDocumentArgs>(jsonConfigFileCreateDocument);
    }

    internal static GetDocumentsArgs GetGetDocumentArgs()
    {
        return Convert<DocumentService.GetDocumentsArgs, GetDocumentsArgs>(jsonConfigFileGetDocuments);
    }

    internal static GetPrivatePersonsArgs GetPrivatePersonsArgs()
    {
        return Convert<ContactService.GetPrivatePersonsArgs, GetPrivatePersonsArgs>(jsonConfigFileGetPrivatePersons);
    }

    internal static GetContactPersonsArgs GetContactPersonsArgs()
    {
        return Convert<ContactService.GetContactPersonsArgs, GetContactPersonsArgs>(jsonConfigFileGetContactPersons);
    }

    internal static SynchronizePrivatePersonArgs GetSynchronizePrivatePersonArgs()
    {
        return Convert<ContactService.SynchronizePrivatePersonArgs, SynchronizePrivatePersonArgs>(jsonConfigFileSynchronizePrivatePerson);
    }

    internal static UpdateDocumentArgs GetUpdateDocumentArgs()
    {
        return Convert<DocumentService.UpdateDocumentArgs, UpdateDocumentArgs>(jsonConfigFileUpdateDocument);
    }

    internal static SignOffDocumentArgs GetSignOffDocumentArgs()
    {
        return Convert<DocumentService.SignOffDocumentArgs, SignOffDocumentArgs>(jsonConfigFileSignOffDocument);
    }

    internal static FileService.UploadArgs GetUploadArgs()
    {
        return JsonConvert.DeserializeObject<FileService.UploadArgs>(jsonConfigFileUploadFile);
    }

    internal static RunResult GetRunResult(string json)
    {
        return JsonConvert.DeserializeObject<RunResult>(json, new JsonSerializerSettings { TypeNameHandling = TypeNameHandling.All });
    }

    private static TResult? Convert<TFrom, TResult>(string document)
        where TFrom : class
        where TResult : class
    {
        dynamic? from = JsonConvert.DeserializeObject<TFrom>(document);
        if (from == null) return default;
        string json = JsonConvert.SerializeObject(from.Parameter);
        TResult? newArgs = JsonConvert.DeserializeObject<TResult>(json);
        return newArgs;
    }
}
