using P360Client.DTO;

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

    internal UpdateDocumentWithFileReferenceStep(string documentNumber, NewDocumentFile fileInput)
        : this(fileInput)
    {
        DocumentNumber = documentNumber;
    }
    internal UpdateDocumentWithFileReferenceStep(NewDocumentFile fileInput) : this()
    {
        FileInput = fileInput;
    }

    public string DocumentNumber { get; set; }
    public NewDocumentFile FileInput { get; set; }

    protected override async Task ExecuteStep(ResourceClient client)
    {
        UpdateDocumentArgs updateDocumentArgs = await JsonDeserializer.GetUpdateDocumentArgsAsync();
        NewDocumentFile? templateFile = updateDocumentArgs.Files?.FirstOrDefault();
        if (templateFile != null)
        {
            templateFile.ExternalId ??= FileInput.ExternalId;
            templateFile.FiledOnPaper ??= FileInput.FiledOnPaper;
            templateFile.Title ??= FileInput.Title;
            templateFile.Status ??= FileInput.Status;
            templateFile.Note ??= FileInput.Note;
            templateFile.PaperLocation ??= FileInput.PaperLocation;
            templateFile.AccessCode ??= FileInput.AccessCode;
            templateFile.Category ??= FileInput.Category;
            templateFile.DegradeCode ??= FileInput.DegradeCode;
            templateFile.DegradeDate ??= FileInput.DegradeDate;
            templateFile.Format ??= FileInput.Format;
            templateFile.PaperLocation ??= FileInput.PaperLocation;
            templateFile.RelationType ??= FileInput.RelationType;
            templateFile.UploadedFileReference ??= FileInput.UploadedFileReference;
            templateFile.Base64Data ??= FileInput.Base64Data;
            templateFile.VersionFormat ??= FileInput.VersionFormat;
            templateFile.Data ??= FileInput.Data;
        }
        else
        {
            List<NewDocumentFile> files = new()
            {
                FileInput
            };
            updateDocumentArgs.Files = files;
        }
        updateDocumentArgs.DocumentNumber = DocumentNumber;
        await client.DocumentResources.UpdateDocumentAsync(updateDocumentArgs);
    }

    protected override async Task ExecuteStep<TStep>(ResourceClient client, TStep fromStep)
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