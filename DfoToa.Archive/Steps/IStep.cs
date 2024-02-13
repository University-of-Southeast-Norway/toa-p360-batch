using P360Client;

namespace DfoToa.Archive.Steps;

public interface IStep
{
    Task Execute(ResourceClient client);
    Task Execute<TStep>(ResourceClient client, TStep fromStep) where TStep : Step;
}