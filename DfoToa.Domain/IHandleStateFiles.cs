using DfoClient;
#if NET48
using System;
using System.Threading.Tasks;
using System.IO;
#endif

namespace DfoToa.Domain
{
    public interface IHandleStateFiles
    {
        Task<string> GetState(Contract contract);
        Task SaveState(Contract contract, string state);

    }
}
