using dfo_toa_manual.DFO;
#if NET48
using System.Threading.Tasks;
#endif

namespace DfoToa.Domain
{
    public interface IEmployeeContractHandler
    {
        Task RunAsync(Employee employee, Contract contract);
    }
}
