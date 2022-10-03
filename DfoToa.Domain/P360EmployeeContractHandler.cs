using dfo_toa_manual.DFO;
using P360Client.Domain;
using System.IO;
#if NET48
using System.Threading.Tasks;
using System.IO;
#endif

namespace DfoToa.Domain
{
    public class P360EmployeeContractHandler : IEmployeeContractHandler
    {
        public IContext Context { get; set; }

        public P360EmployeeContractHandler(IContext context)
        {
            Context = context;
        }

        public async Task RunAsync(Employee employee, Contract contract)
        {
            DocumentService.Files2 contractFile = new DocumentService.Files2();
            contractFile.Title = contract.ContractId;
            contractFile.Format = "pdf";
            contractFile.Base64Data = contract.FileContent;
            P360BusinessLogic.Init(Context);
            var runResult = new RunResult();
            await P360BusinessLogic.Run(runResult, employee.SocialSecurityNumber, employee.FirstName, null, employee.LastName, employee.Address, employee.Zipcode, employee.City, employee.PhoneNumber, employee.Email, contractFile);
        }
    }
}
