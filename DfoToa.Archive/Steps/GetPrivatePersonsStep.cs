using P360Client;
using static DfoToa.Archive.Steps.SynchronizePersonStep;
using static DfoToa.Archive.Steps.TryFindCaseStep;

namespace DfoToa.Archive.Steps;

public class GetPrivatePersonsStep : Step, IHaveTryFindCaseStepDependencies, IHaveSynchronizePersonStepDependencies
{
    public interface IHaveGetPrivatePersonsStepDependencies : IStep
    {
        string PersonlIdNumber { get; }
    }

    internal GetPrivatePersonsStep(string personlIdNumber) : this()
    {
        PersonlIdNumber = personlIdNumber;
    }
    internal GetPrivatePersonsStep() : base(nameof(GetPrivatePersonsStep))
    {
    }

    public ICollection<ContactService.PrivatePersons> PrivatePersons { get; set; }
    public string PersonlIdNumber { get; set; }

    public int? Recno { get; set; }

    protected override async Task ExecuteStep(Client client)
    {
        var getPrivatePersonsArgs = JsonDeserializerObsolete.GetPrivatePersonsArgs();
        getPrivatePersonsArgs.Parameter.PersonalIdNumber = PersonlIdNumber;
        PrivatePersons = await client.GetPrivatePersonsAsync(getPrivatePersonsArgs);
        Recno = PrivatePersons?.FirstOrDefault()?.Recno; // TODO: Find better solution to this logic. It assumes that we only found one PrivatePersons.
    }

    protected override async Task ExecuteStep<TStep>(Client client, TStep fromStep)
    {
        if (fromStep is IHaveGetPrivatePersonsStepDependencies step)
        {
            PersonlIdNumber = step.PersonlIdNumber;
            await ExecuteStep(client);
            return;
        }
        throw new Exception($"Can't execute from step {fromStep.GetType()}. '{nameof(fromStep)}' must implement {typeof(IHaveGetPrivatePersonsStepDependencies)}.");
    }
}