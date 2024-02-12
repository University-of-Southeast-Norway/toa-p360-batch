using P360Client;

namespace DfoToa.Archive.Steps;

public class UpdateDocumentWithFileReferenceStep : Step, SignOffDocumentStep.IHaveSignOffDocumentStepDependencies
{
    public interface IHaveUpdateDocumentWithFileReferenceStepDependencies : IStep
    {
        string DocumentNumber { get; }
    }

    internal UpdateDocumentWithFileReferenceStep()
        : base(nameof(UpdateDocumentWithFileReferenceStep))
    {

    }

    internal UpdateDocumentWithFileReferenceStep(string documentNumber, DocumentService.Files2 fileInput)
        : this(fileInput)
    {
        DocumentNumber = documentNumber;
    }
    internal UpdateDocumentWithFileReferenceStep(DocumentService.Files2 fileInput) : this()
    {
        FileInput = fileInput;
    }

    public string DocumentNumber { get; set; }
    public DocumentService.Files2 FileInput { get; set; }

    protected override async Task ExecuteStep(Client client)
    {
        var updateDocumentArgs = JsonDeserializerObsolete.GetUpdateDocumentArgs();
        DocumentService.Files2 templateFile = updateDocumentArgs.Parameter.Files?.FirstOrDefault();
        if (templateFile != null)
        {
            templateFile.Title = templateFile.Title ?? FileInput.Title;
            templateFile.Status = templateFile.Status ?? FileInput.Status;
            templateFile.Note = templateFile.Note ?? FileInput.Note;
            templateFile.PaperLocation = templateFile.PaperLocation ?? FileInput.PaperLocation;
            templateFile.AccessCode = templateFile.AccessCode ?? FileInput.AccessCode;
            templateFile.AdditionalFields = templateFile.AdditionalFields ?? FileInput.AdditionalFields;
            templateFile.Category = templateFile.Category ?? FileInput.Category;
            templateFile.DegradeCode = templateFile.DegradeCode ?? FileInput.DegradeCode;
            templateFile.DegradeDate = templateFile.DegradeDate ?? FileInput.DegradeDate;
            templateFile.FiledOnPaper = templateFile.FiledOnPaper ?? FileInput.FiledOnPaper;
            templateFile.Format = templateFile.Format ?? FileInput.Format;
            templateFile.PaperLocation = templateFile.PaperLocation ?? FileInput.PaperLocation;
            templateFile.RelationType = templateFile.RelationType ?? FileInput.RelationType;
            templateFile.UploadedFileReference = templateFile.UploadedFileReference ?? FileInput.UploadedFileReference;
            templateFile.Base64Data = templateFile.Base64Data ?? FileInput.Base64Data;
            templateFile.VersionFormat = templateFile.VersionFormat ?? FileInput.VersionFormat;
            templateFile.Data = templateFile.Data ?? FileInput.Data;
        }
        else
        {
            ICollection<DocumentService.Files2> files = new List<DocumentService.Files2>
            {
                FileInput
            };
            updateDocumentArgs.Parameter.Files = files;
        }
        updateDocumentArgs.Parameter.DocumentNumber = DocumentNumber;
        await client.UpdateDocumentAsync(updateDocumentArgs);
    }

    protected override async Task ExecuteStep<TStep>(Client client, TStep fromStep)
    {
        if (fromStep is IHaveUpdateDocumentWithFileReferenceStepDependencies step)
        {
            DocumentNumber = step.DocumentNumber;
            await ExecuteStep(client);
            return;
        }
        throw new Exception($"Can't execute from step {fromStep.GetType()}. '{nameof(fromStep)}' must implement {typeof(IHaveUpdateDocumentWithFileReferenceStepDependencies)}.");
    }

    public override string ToString()
    {
        return $"{nameof(UpdateDocumentWithFileReferenceStep)}:{FileInput.Title}.{FileInput.Format}";
    }
}