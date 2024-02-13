using P360Client.Domain;
using P360Client.DTO;
using static DfoToa.Archive.Steps.CreateCaseStep;
using static DfoToa.Archive.Steps.CreateDocumentStep;

namespace DfoToa.Archive.Steps;

public class TryFindCaseStep : Step, IHaveCreateDocumentStepDependencies, IHaveCreateCaseStepDependencies
{
    public interface IHaveTryFindCaseStepDependencies : IStep
    {
        int? Recno { get; }
    }

    internal TryFindCaseStep() : base(nameof(TryFindCaseStep))
    {

    }

    internal TryFindCaseStep(DateTimeOffset inProductionDate, int recno) : this(inProductionDate)
    {
        Recno = recno;

    }
    internal TryFindCaseStep(DateTimeOffset inProductionDate) : this()
    {
        InProductionDate = inProductionDate;
    }

    public Case FoundCase { get; set; }
    public int? Recno { get; set; }
    public DateTimeOffset InProductionDate { get; set; }
    public string CaseNumber { get; set; }

    protected override async Task ExecuteStep(ResourceClient client)
    {
        if (Recno == null) throw new Exception($"{nameof(Recno)} is null. Consider calling {nameof(TryFindCaseStep)}({typeof(ResourceClient)},{typeof(DateTimeOffset)},{typeof(int)}) if this is the first step.");
        Case foundCase = null;
        IEnumerable<Case> foundPersonCases = await GetCasesOnPerson(client, Recno.Value);
        foreach (Case personCase in foundPersonCases?.OrderByDescending(f => f.CreatedDate))
        {
            if (personCase.CreatedDate <= InProductionDate && personCase.Status != "Under behandling" && personCase.Status != "In progress")
            {
                continue;
            }

            foundCase = personCase;
            break;
        }

        FoundCase = foundCase;
        CaseNumber = FoundCase?.CaseNumber;
    }

    protected override async Task ExecuteStep<TStep>(ResourceClient client, TStep fromStep)
    {
        if (fromStep is IHaveTryFindCaseStepDependencies step)
        {
            Recno = step.Recno;
            await ExecuteStep(client);
            return;
        }
        throw new Exception($"Can't execute from step {fromStep.GetType()}. '{nameof(fromStep)}' must implement {typeof(IHaveTryFindCaseStepDependencies)}.");
    }

    private async Task<IEnumerable<Case>> GetCasesOnPerson(ResourceClient client, int recno)
    {
        var getCasesArgs = await JsonDeserializer.GetGetCaseArgsAsync();
        getCasesArgs.ContactRecnos = new List<int> { recno };
        IEnumerable<Case> result = await client.CaseResources.GetCasesAsync(getCasesArgs);
        return result;
    }
}