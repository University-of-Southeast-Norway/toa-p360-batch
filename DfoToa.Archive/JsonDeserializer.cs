using Newtonsoft.Json;
using P360Client.DTO;
using System.Text;
using File = System.IO.File;

namespace DfoToa.Archive;

internal class JsonDeserializer
{
    private readonly static string jsonConfigFileSynchronizePrivatePerson = File.ReadAllText(@"Definitions/synchronize_private_person.json", Encoding.UTF8);
    private readonly static string jsonConfigFileSynchronizeContactPerson = File.ReadAllText(@"Definitions/synchronize_contact_person.json", Encoding.UTF8);
    private readonly static string jsonConfigFileGetPrivatePersons = File.ReadAllText(@"Definitions/get_private_persons.json", Encoding.UTF8);
    private readonly static string jsonConfigFileGetContactPersons = File.ReadAllText(@"Definitions/get_contact_persons.json", Encoding.UTF8);
    private readonly static string jsonConfigFileCreateCase = File.ReadAllText(@"Definitions/create_case.json", Encoding.UTF8);
    private readonly static string jsonConfigFileGetCases = File.ReadAllText(@"Definitions/get_cases.json", Encoding.UTF8);
    private readonly static string jsonConfigFileCreateDocument = File.ReadAllText(@"Definitions/create_document.json", Encoding.UTF8);
    private readonly static string jsonConfigFileUploadFile = File.ReadAllText(@"Definitions/upload_file.json", Encoding.UTF8);
    private readonly static string jsonConfigFileUpdateDocument = File.ReadAllText(@"Definitions/update_document.json", Encoding.UTF8);
    private readonly static string jsonConfigFileSignOffDocument = File.ReadAllText(@"Definitions/sign_off_document.json", Encoding.UTF8);
    private readonly static string jsonConfigFileGetDocuments = File.ReadAllText(@"Definitions/get_documents.json", Encoding.UTF8);

    internal static GetCasesArgs GetGetCaseArgs()
    {
        return JsonConvert.DeserializeObject<ParameterTemplate<GetCasesArgs>>(jsonConfigFileGetCases).Parameter;
    }

    internal static CreateCaseArgs GetCreateCaseArgs()
    {
        return JsonConvert.DeserializeObject<ParameterTemplate<CreateCaseArgs>>(jsonConfigFileCreateCase).Parameter;
    }

    internal static CreateDocumentArgs GetCreateDocumentArgs()
    {
        return JsonConvert.DeserializeObject<ParameterTemplate<CreateDocumentArgs>>(jsonConfigFileCreateDocument).Parameter;
    }

    internal static GetDocumentsArgs GetGetDocumentArgs()
    {
        return JsonConvert.DeserializeObject<ParameterTemplate<GetDocumentsArgs>>(jsonConfigFileGetDocuments).Parameter;
    }

    internal static GetPrivatePersonsArgs GetPrivatePersonsArgs()
    {
        return JsonConvert.DeserializeObject<ParameterTemplate<GetPrivatePersonsArgs>>(jsonConfigFileGetPrivatePersons).Parameter;
    }

    internal static GetContactPersonsArgs GetContactPersonsArgs()
    {
        return JsonConvert.DeserializeObject<ParameterTemplate<GetContactPersonsArgs>>(jsonConfigFileGetContactPersons).Parameter;
    }

    internal static SynchronizePrivatePersonArgs GetSynchronizePrivatePersonArgs()
    {
        return JsonConvert.DeserializeObject<ParameterTemplate<SynchronizePrivatePersonArgs>>(jsonConfigFileSynchronizePrivatePerson).Parameter;
    }

    internal static SynchronizeContactPersonArgs GetSynchronizeContactPersonArgs()
    {
        return JsonConvert.DeserializeObject<ParameterTemplate<SynchronizeContactPersonArgs>>(jsonConfigFileSynchronizeContactPerson).Parameter;
    }

    internal static UpdateDocumentArgs GetUpdateDocumentArgs()
    {
        return JsonConvert.DeserializeObject<ParameterTemplate<UpdateDocumentArgs>>(jsonConfigFileUpdateDocument).Parameter;
    }

    internal static SignOffDocumentArgs GetSignOffDocumentArgs()
    {
        return JsonConvert.DeserializeObject<ParameterTemplate<SignOffDocumentArgs>>(jsonConfigFileSignOffDocument).Parameter;
    }

    internal static RunResult GetRunResult(string json)
    {
        return JsonConvert.DeserializeObject<RunResult>(json, new JsonSerializerSettings { TypeNameHandling = TypeNameHandling.All });
    }
}
public class ParameterTemplate<TParameter>
{
    public TParameter Parameter { get; set; }
}
