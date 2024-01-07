using DfoClient;

namespace DfoToa.Domain;

public interface IHandleStateFiles
{
    Task<string> GetState(Contract contract);
    Task SaveState(Contract contract, string state);

}
