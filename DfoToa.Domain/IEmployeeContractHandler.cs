using DfoClient;
using DfoClient.Dto;
namespace DfoToa.Domain;

public interface IEmployeeContractHandler
{
    Task RunAsync(Employee employee, Contract contract, EmployeeContract employeeContract, Employee? caseManager);
}
