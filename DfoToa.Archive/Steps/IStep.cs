using P360Client;

namespace DfoToa.Archive.Steps;

public interface IStep
{
    Task Execute(Client client);
    Task Execute<TStep>(Client client, TStep fromStep) where TStep : Step;
}