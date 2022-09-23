using dfo_toa_manual.DFO;
using P360Client.Domain;
#if NET48
using System.Threading.Tasks;
#endif

namespace DfoToa.Domain
{
    public class EmployeeContractHandler : IEmployeeContractHandler
    {
        public IContext Context { get; set; }

        public EmployeeContractHandler(IContext context)
        {
            Context = context;
        }

        public async Task RunAsync(Employee employee, Contract contract)
        {
            DocumentService.Files2 contractFile = new DocumentService.Files2();
            contractFile.Title = contract.ContractId;
            contractFile.Format = "pdf";
            contractFile.Base64Data = contract.FileContent;
            P360BusinessLogic.Context = Context;
            await P360BusinessLogic.Run(employee.SocialSecurityNumber, employee.FirstName, null, employee.LastName, employee.Address, employee.Zipcode, employee.City, employee.PhoneNumber, employee.Email, contractFile);
        }
    }
}
