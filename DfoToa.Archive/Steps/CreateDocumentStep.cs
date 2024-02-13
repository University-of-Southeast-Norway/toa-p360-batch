using static DfoToa.Archive.Steps.UpdateDocumentWithFileReferenceStep;

namespace DfoToa.Archive.Steps;

public class CreateDocumentStep : Step, IHaveUpdateDocumentWithFileReferenceStepDependencies
{
    public interface IHaveCreateDocumentStepDependencies : IStep
    {
        string CaseNumber { get; }
        int? Recno { get; }
    }

    internal CreateDocumentStep(string caseNumber, int? recno, DateTimeOffset? documentDate) : this(documentDate)
    {
        CaseNumber = caseNumber;
        Recno = recno;
    }
    internal CreateDocumentStep(DateTimeOffset? documentDate) : base(nameof(CreateDocumentStep))
    {
        DocumentDate = documentDate;
    }
    internal CreateDocumentStep() : base(nameof(CreateDocumentStep))
    {
    }

    public string DocumentNumber { get; set; }
    public string CaseNumber { get; set; }
    public int? Recno { get; set; }
    public DateTimeOffset? DocumentDate { get; set; }

    protected override async Task ExecuteStep(ResourceClient client)
    {
        var createDocumentArgs = await JsonDeserializer.GetCreateDocumentArgsAsync();
        createDocumentArgs.CaseNumber = CaseNumber;
        createDocumentArgs.DocumentDate = DocumentDate ?? DateTimeOffset.Now.Date;
        P360Client.DTO.ContactReference? submitter = createDocumentArgs.Contacts.FirstOrDefault(f => f.Role == "Avsender");
        if (submitter != null && Recno.HasValue) submitter.ReferenceNumber = "recno:" + Recno;
        DocumentNumber = await client.DocumentResources.CreateDocumentAsync(createDocumentArgs);
    }

    protected override async Task ExecuteStep<TStep>(ResourceClient client, TStep fromStep)
    {
        if (fromStep is IHaveCreateDocumentStepDependencies step)
        {
            CaseNumber = step.CaseNumber;
            Recno = step.Recno;
            await ExecuteStep(client);
            return;
        }
        throw new Exception($"Can't execute from step {fromStep.GetType()}. '{nameof(fromStep)}' must implement {typeof(IHaveCreateDocumentStepDependencies)}.");
    }

    public override string ToString()
    {
        return $"{nameof(CreateDocumentStep)}:{nameof(DocumentNumber)}:{DocumentNumber}";
    }
}