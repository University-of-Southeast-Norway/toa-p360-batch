using P360Client;
using static DfoToa.Archive.Steps.CreateDocumentStep;

namespace DfoToa.Archive.Steps;

public class CreateCaseStep : Step, IHaveCreateDocumentStepDependencies
{
    public interface IHaveCreateCaseStepDependencies : IStep
    {
        int? Recno { get; }
    }

    internal CreateCaseStep(int recno) : this()
    {
        Recno = recno;
    }
    internal CreateCaseStep() : base(nameof(CreateCaseStep))
    {

    }

    public string CaseNumber { get; set; }
    public int? Recno { get; set; }

    protected override async Task ExecuteStep(Client client)
    {
        if (Recno == null) throw new Exception($"{nameof(Recno)} is null. Consider calling {nameof(CreateCaseStep)}({typeof(Client)},{typeof(int)}) if this is the first step.");
        var createCaseArgs = JsonDeserializerObsolete.GetCreateCaseArgs();
        if (createCaseArgs.Parameter.Contacts != null)
        {
            createCaseArgs.Parameter.Contacts.First().ReferenceNumber = "recno:" + Recno;
        }
        CaseNumber = await client.CreateCaseAsync(createCaseArgs);
    }

    protected override async Task ExecuteStep<TStep>(Client client, TStep fromStep)
    {
        if (fromStep is IHaveCreateCaseStepDependencies step)
        {
            Recno = step.Recno;
            await ExecuteStep(client);
            return;
        }
        throw new Exception($"Can't execute from step {fromStep.GetType()}. '{nameof(fromStep)}' must implement {typeof(IHaveCreateCaseStepDependencies)}.");
    }

    public override string ToString()
    {
        return $"{nameof(CreateCaseStep)}:{nameof(CaseNumber)}:{CaseNumber}";
    }
}