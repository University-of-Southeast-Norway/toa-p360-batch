using P360Client.Domain;

namespace DfoToa.Archive.Steps;

public class SignOffDocumentStep : Step
{
    public interface IHaveSignOffDocumentStepDependencies
    {
        string DocumentNumber { get; }
    }

    public SignOffDocumentStep() : base(nameof(SignOffDocumentStep))
    {
    }

    public SignOffDocumentStep(string documentNumber) : this()
    {
        DocumentNumber = documentNumber;
    }

    public string DocumentNumber { get; set; }

    protected override async Task ExecuteStep(ResourceClient client)
    {
        var signOffDocumentStep = await JsonDeserializer.GetSignOffDocumentArgsAsync();
        signOffDocumentStep.Document = DocumentNumber;
        await client.DocumentResources.SignOffDocumentAsync(signOffDocumentStep);
    }

    protected override Task ExecuteStep<TStep>(ResourceClient client, TStep fromStep)
    {
        if (fromStep is IHaveSignOffDocumentStepDependencies step)
        {
            DocumentNumber = step.DocumentNumber;
            return ExecuteStep(client);
        }
        throw new Exception($"Can't execute from step {fromStep.GetType()}. Previous step must implement {typeof(IHaveSignOffDocumentStepDependencies)}.");
    }

    public override string ToString()
    {
        return $"{nameof(SynchronizePersonStep)}:{nameof(DocumentNumber)}:{DocumentNumber}";
    }
}