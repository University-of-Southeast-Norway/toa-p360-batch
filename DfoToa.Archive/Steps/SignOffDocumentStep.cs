using P360Client;

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

    protected override Task ExecuteStep(Client client)
    {
        var signOffDocumentStep = JsonDeserializerObsolete.GetSignOffDocumentArgs();
        signOffDocumentStep.Parameter.Document = DocumentNumber;
        return client.SignOffDocumentAsync(signOffDocumentStep);
    }

    protected override Task ExecuteStep<TStep>(Client client, TStep fromStep)
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