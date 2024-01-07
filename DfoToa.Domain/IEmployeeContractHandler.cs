using DfoClient;
namespace DfoToa.Domain;

public interface IEmployeeContractHandler
{
    Task RunAsync(Employee employee, Contract contract);
}
