using P360Client.DTO;
using static DfoToa.Archive.Steps.UpdateDocumentWithFileReferenceStep;

namespace DfoToa.Archive.Steps;

public class CreateDocumentStep : Step, IHaveUpdateDocumentWithFileReferenceStepDependencies
{
    public interface IHaveCreateDocumentStepDependencies : IStep
    {
        string CaseNumber { get; }
        int? Recno { get; }
    }

    internal CreateDocumentStep(string caseNumber, int? recno, DateTimeOffset? documentDate, PrivatePerson? responsiblePerson, IEnumerable<PrivatePerson>? receivers) : this(documentDate, responsiblePerson, receivers)
    {
        CaseNumber = caseNumber;
        Recno = recno;
    }
    internal CreateDocumentStep(DateTimeOffset? documentDate, PrivatePerson? responsiblePerson, IEnumerable<PrivatePerson>? receivers) : base(nameof(CreateDocumentStep))
    {
        DocumentDate = documentDate;
        ResponsiblePerson = responsiblePerson;
        Receivers = receivers;
    }
    internal CreateDocumentStep() : base(nameof(CreateDocumentStep))
    {
    }

    public string DocumentNumber { get; set; }
    public string CaseNumber { get; set; }
    public int? Recno { get; set; }
    public DateTimeOffset? DocumentDate { get; set; }
    public IEnumerable<PrivatePerson>? Receivers { get; set; }
    public PrivatePerson? ResponsiblePerson { get; set; }

    protected override async Task ExecuteStep(ResourceClient client)
    {
        var createDocumentArgs = await JsonDeserializer.GetCreateDocumentArgsAsync();
        createDocumentArgs.CaseNumber = CaseNumber;
        createDocumentArgs.DocumentDate = DocumentDate ?? DateTimeOffset.Now.Date;
        ContactReference? submitter = createDocumentArgs.Contacts.FirstOrDefault(f => f.Role == "Avsender");
        if (submitter != null && Recno.HasValue) submitter.ReferenceNumber = $"recno:{Recno}";

        if (ResponsiblePerson is not null)
        {
            createDocumentArgs.ResponsiblePersonRecno ??= await GetContactPersonRecno(client, ResponsiblePerson);
        }
        foreach (PrivatePerson receiver in Receivers ?? Array.Empty<PrivatePerson>())
        {
            int? recnoCaseManager = await GetContactPersonRecno(client, receiver);
            if (recnoCaseManager.HasValue)
            {
                (createDocumentArgs.Contacts ??= new List<ContactReference>()).Add(new ContactReference { ReferenceNumber = $"recno:{recnoCaseManager}", Role = "Mottaker" });
            }
        }
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
    private static async Task<int?> GetContactPersonRecno(ResourceClient client, PrivatePerson receiver)
    {
        var getContactPersonsArgs = await JsonDeserializer.GetContactPersonsArgsAsync();
        getContactPersonsArgs.ExternalId = receiver.ExternalID;
        getContactPersonsArgs.Email = receiver.Email;
        IEnumerable<PrivatePerson> persons = await client.ContactResources.GetContactPersonsAsync(getContactPersonsArgs);
        return persons?.FirstOrDefault()?.Recno; // TODO: Find better solution to this logic. It assumes that we only found one PrivatePersons.
    }

    public override string ToString()
    {
        return $"{nameof(CreateDocumentStep)}:{nameof(DocumentNumber)}:{DocumentNumber}";
    }
}