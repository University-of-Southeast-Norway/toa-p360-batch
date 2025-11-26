using P360Client.Resources;

namespace DfoToa.Archive;

public interface IContext : P360Client.IContext
{
    string InProductionDate { get; }
    ICaseResources CaseResources { get; }
    IDocumentResources DocumentResources { get; }
    IContactResources ContactResources { get; }
    ISupportResources SupportResources { get; }
}
