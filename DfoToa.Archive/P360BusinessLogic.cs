using Newtonsoft.Json.Linq;
using DfoToa.Archive.Steps;
using P360Client;


#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
namespace DfoToa.Archive;

public static class P360BusinessLogic
{
    private static IContext _context;
    private static Client _client;

    public static void Init(IContext context)
    {
        Init(context, new Client(context));
    }

    public static void Init(IContext context, Client client)
    {
        _context = context;
        _client = client;
    }

    public static async Task RunUploadFileToPrivatePerson(RunResult runResult, string personalIdNumber, string firstName, string middleName, string lastName, string streetAddress, string zipCode, string zipPlace, string mobilePhoneNumber, string email, DocumentService.Files2 fileInput, DateTimeOffset? documentDate = null)
    {
        CheckState();

        DateTimeOffset inProductionDate = DateTimeOffset.Parse(_context.InProductionDate);

        var privatePersons = await GetPrivatePersons(personalIdNumber);

        if (privatePersons.Count == 0)
        {
            WhenNoExistingPersonFound(runResult, personalIdNumber, firstName, middleName, lastName, streetAddress, zipCode, zipPlace, mobilePhoneNumber, email, fileInput, documentDate);
        }
        else if (privatePersons.Count > 1)
        {
            WhenToManyPersonsFound(personalIdNumber, privatePersons);
            return;
        }
        else
        {
            await WhenUniquePersonFound(runResult, fileInput, inProductionDate, privatePersons.Single(), documentDate);
        }


        await Execute(runResult);
    }

    private static async Task<ICollection<ContactService.PrivatePersons>> GetPrivatePersons(string personlIdNumber)
    {
        var getPrivatePersonsArgs = JsonDeserializerObsolete.GetPrivatePersonsArgs();
        getPrivatePersonsArgs.Parameter.PersonalIdNumber = personlIdNumber;
        return await _client.GetPrivatePersonsAsync(getPrivatePersonsArgs);
    }

    private static async Task WhenUniquePersonFound(RunResult runResult, DocumentService.Files2 fileInput, DateTimeOffset inProductionDate, ContactService.PrivatePersons privatePerson, DateTimeOffset? documentDate = null)
    {
        if (!privatePerson.Recno.HasValue) throw new Exception($"{nameof(ContactService.PrivatePersons.Recno)} is null. This property is required to have value.");
        _context.CurrentLogger.WriteToLog($"Found one person with social security number {privatePerson.PersonalIdNumber} with recno {privatePerson.Recno}.");

        CaseService.Cases foundCase = await FindCase(_client, privatePerson.Recno.Value, inProductionDate);
        if (foundCase == null)
        {
            _context.CurrentLogger.WriteToLog($"No case found on {privatePerson.PersonalIdNumber}. New case will be created.");
            runResult.AddCreateCaseStep(privatePerson.Recno.Value);
            runResult.AddCreateDocumentStep(documentDate);
        }
        else
        {
            _context.CurrentLogger.WriteToLog($"Existing case, with case number {foundCase.CaseNumber}, found on person with social security number {privatePerson.PersonalIdNumber}.");
            ICollection<DocumentService.Documents> documents = await FindDocuments(foundCase.CaseNumber);

            if (documents?.Any(d => d.Files is JContainer jsonFiles
                && jsonFiles?.First?.ToObject(typeof(DocumentService.Files2)) is DocumentService.Files2 file
                && fileInput.Note != null && file.Note == fileInput.Note) == true)
            {
                _context.CurrentLogger.WriteToLog($"File being uploaded is already attached to document {documents.First().DocumentNumber}.");
                return;
            }
            runResult.AddCreateDocumentStep(foundCase.CaseNumber, privatePerson.Recno, documentDate);
        }
        AddCommonSteps(runResult, fileInput);
    }

    private static void AddCommonSteps(RunResult runResult, DocumentService.Files2 fileInput)
    {
        runResult.AddUpdateDocumentWithFileReferenceStep(fileInput);
        runResult.AddSignOffDocumentStep();
    }

    private static async Task Execute(RunResult runResult)
    {
        await runResult.Execute(_client);

        if (runResult.Steps.All(s => s.Success))
        {
            if (runResult.Steps.FirstOrDefault(s => s is SynchronizePersonStep) is SynchronizePersonStep synchronizePersonStep)
            {
                _context.CurrentLogger.WriteToLog($"Person created with recno {synchronizePersonStep.Recno}.");
            }

            if (runResult.Steps.FirstOrDefault(s => s is CreateCaseStep) is CreateCaseStep createCaseStep)
            {
                _context.CurrentLogger.WriteToLog($"Case {createCaseStep.CaseNumber} created.");
            }

            if (runResult.Steps.FirstOrDefault(s => s is CreateDocumentStep) is CreateDocumentStep createDocumentStep)
            {
                _context.CurrentLogger.WriteToLog($"Document {createDocumentStep.DocumentNumber} added to case {createDocumentStep.CaseNumber}.");
            }
        }
    }

    private static async Task<ICollection<DocumentService.Documents>> FindDocuments(string caseNumber)
    {
        var getDocumentsArgs = JsonDeserializerObsolete.GetGetDocumentArgs();
        getDocumentsArgs.Parameter.CaseNumber = caseNumber;

        return await _client.GetDocumentsAsync(getDocumentsArgs);
    }

    private static async Task<CaseService.Cases> FindCase(Client client, int recno, DateTimeOffset inProductionDate)
    {
        CaseService.Cases foundCase = null;
        ICollection<CaseService.Cases> foundPersonCases = await GetCasesOnPerson(client, recno);
        foreach (CaseService.Cases personCase in foundPersonCases?.OrderByDescending(f => f.CreatedDate))
        {
            if (personCase.CreatedDate <= inProductionDate && personCase.Status != "Under behandling" && personCase.Status != "In progress")
            {
                continue;
            }

            foundCase = personCase;
            break;
        }
        return foundCase;
    }

    private static async Task<ICollection<CaseService.Cases>> GetCasesOnPerson(Client client, int recno)
    {
        var getCasesArgs = JsonDeserializerObsolete.GetGetCaseArgs();
        getCasesArgs.Parameter.ContactRecnos = new List<int> { recno };
        ICollection<CaseService.Cases> result = await client.GetCasesAsync(getCasesArgs);
        return result;
    }

    private static void WhenToManyPersonsFound(string personalIdNumber, ICollection<ContactService.PrivatePersons> privatePersons)
    {
        _context.CurrentLogger.WriteToLog($"Found 2 or more persons with social security number {personalIdNumber} with the following recno:");
        foreach (var person in privatePersons)
        {
            _context.CurrentLogger.WriteToLog($"Recno:{person.Recno}");
        }
    }

    private static void WhenNoExistingPersonFound(RunResult runResult, string personalIdNumber, string firstName, string middleName, string lastName, string streetAddress, string zipCode, string zipPlace, string mobilePhoneNumber, string email, DocumentService.Files2 fileInput, DateTimeOffset? documentDate = null)
    {
        _context.CurrentLogger.WriteToLog($"Couldn't find any person that match social security number {personalIdNumber}.");

        runResult.AddSynchronizePersonStep(personalIdNumber, firstName, middleName, lastName, streetAddress, zipCode, zipPlace, mobilePhoneNumber, email);
        runResult.AddCreateCaseStep();
        runResult.AddCreateDocumentStep(documentDate);
        AddCommonSteps(runResult, fileInput);
    }

    public static async Task<RunResult> Run(RunResult runResult)
    {
        CheckState();
        await runResult.Execute(_client);
        return runResult;
    }

    public static string GetJsonFromRunResult(RunResult runResult)
    {
        return JsonSerializer.FromRunResult(runResult);
    }

    public static RunResult GetRunResultFromJson(string json)
    {
        return JsonDeserializer.GetRunResult(json);
    }

    private static void CheckState()
    {
        if (_client == null) throw new Exception($"One of {nameof(Init)} must be called before Run can be executed.");
    }
}
