using P360Client.Resources;


#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
namespace DfoToa.Archive;

public class ResourceClient
{
    public ResourceClient(ICaseResources caseResources, IDocumentResources documentResources, IContactResources contactResources, ISupportResources supportResources)
    {
        CaseResources = caseResources;
        DocumentResources = documentResources;
        ContactResources = contactResources;
        SupportResources = supportResources;
    }
    public ICaseResources CaseResources { get; set; }
    public IDocumentResources DocumentResources { get; set; }
    public IContactResources ContactResources { get; set; }
    public ISupportResources SupportResources { get; set; }
}
