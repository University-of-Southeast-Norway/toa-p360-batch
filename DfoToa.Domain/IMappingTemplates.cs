namespace DfoToa.Domain;

public interface IMappingTemplates
{
    Task<UniqueQueryAttributesTemplate?> GetResponsibleTemplate();
    Task<UniqueTitlesTemplate?> GetTitlesTemplate();
}
