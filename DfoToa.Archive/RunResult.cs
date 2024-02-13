using DfoToa.Archive.Steps;
using P360Client;
using P360Client.DTO;
using System.Text;

namespace DfoToa.Archive;

public class RunResult
{
    public ICollection<Step> Steps { get; set; } = new List<Step>();

    public RunResult()
    {
    }

    public async Task Execute(ResourceClient client)
    {
        Step previousStep = null;
        foreach (var step in Steps)
        {
            if (previousStep == null)
            {
                await step.Execute(client);
            }
            else if (!previousStep.Success) return;
            else
            {
                await step.Execute(client, previousStep);
            }
            previousStep = step;
        }
    }

    public bool Success => Steps.Any() && Steps.All(s => s.Success);

    public bool Unfinished => Steps.Any(s => !s.Success);

    internal CreateDocumentStep AddCreateDocumentStep(DateTimeOffset? documentDate)
    {
        return AddStep(new CreateDocumentStep(documentDate));
    }
    internal CreateDocumentStep AddCreateDocumentStep(string caseNumber, int? recno, DateTimeOffset? documentDate = null)
    {
        return AddStep(new CreateDocumentStep(caseNumber, recno, documentDate));
    }

    internal UpdateDocumentWithFileReferenceStep AddUpdateDocumentWithFileReferenceStep(NewDocumentFile fileInput)
    {
        return AddStep(new UpdateDocumentWithFileReferenceStep(fileInput));
    }
    internal UpdateDocumentWithFileReferenceStep AddUpdateDocumentWithFileReferenceStep(string documentNumber, NewDocumentFile fileInput)
    {
        return AddStep(new UpdateDocumentWithFileReferenceStep(documentNumber, fileInput));
    }

    internal SynchronizePersonStep AddSynchronizePersonStep(string firstName, string middleName, string lastName, string streetAddress, string zipCode, string zipPlace, string mobilePhoneNumber, string email)
    {
        return AddStep(new SynchronizePersonStep(firstName, middleName, lastName, streetAddress, zipCode, zipPlace, mobilePhoneNumber, email));
    }
    internal SynchronizePersonStep AddSynchronizePersonStep(string personlIDNumber, string firstName, string middleName, string lastName, string streetAddress, string zipCode, string zipPlace, string mobilePhoneNumber, string email)
    {
        return AddStep(new SynchronizePersonStep(personlIDNumber, firstName, middleName, lastName, streetAddress, zipCode, zipPlace, mobilePhoneNumber, email));
    }

    internal CreateCaseStep AddCreateCaseStep(int recno)
    {
        return AddStep(new CreateCaseStep(recno));
    }
    internal CreateCaseStep AddCreateCaseStep()
    {
        return AddStep(new CreateCaseStep());
    }

    internal GetPrivatePersonsStep AddGetPrivatePersonsStep(string personlIdNumber)
    {
        return AddStep(new GetPrivatePersonsStep(personlIdNumber));
    }

    internal SignOffDocumentStep AddSignOffDocumentStep()
    {
        return AddStep(new SignOffDocumentStep());
    }

    internal SignOffDocumentStep AddSignOffDocumentStep(string documentNumber)
    {
        return AddStep(new SignOffDocumentStep(documentNumber));
    }

    internal GetPrivatePersonsStep AddGetPrivatePersonsStep()
    {
        return AddStep(new GetPrivatePersonsStep());
    }

    internal TryFindCaseStep AddTryFindCaseStep(DateTimeOffset inProductionDate, int recno)
    {
        return AddStep(new TryFindCaseStep(inProductionDate, recno));
    }
    internal TryFindCaseStep AddTryFindCaseStep(DateTimeOffset inProductionDate)
    {
        return AddStep(new TryFindCaseStep(inProductionDate));
    }

    internal T AddStep<T>(T step) where T : Step
    {
        Steps.Add(step);
        return step;
    }

    public override string ToString()
    {
        return ToString(";");
    }

    public string ToString(string separator)
    {
        StringBuilder sb = new StringBuilder();
        foreach (var step in Steps)
        {
            sb.Append(step).Append(separator);
        }
        return sb.ToString();
    }
}