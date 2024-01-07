using DfoClient;
using P360Client.Domain;

namespace DfoToa.Domain;

public class P360EmployeeContractHandler : IEmployeeContractHandler
{
    public IContext Context { get; set; }

    public P360EmployeeContractHandler(IContext context)
    {
        Context = context;
    }

    public async Task RunAsync(Employee employee, Contract contract)
    {
        DocumentService.Files2 contractFile = new DocumentService.Files2
        {
            Title = $"Signert avtale {contract.SequenceNumber}",
            Format = "pdf",
            Note = Utility.CreateChecksum(contract.FileContent),
            Base64Data = contract.FileContent
        };
        P360BusinessLogic.Init(Context);
        string existingState = await Context.StateFileHandler.GetState(contract);
        RunResult runResult = null;

        if (!string.IsNullOrEmpty(existingState)) runResult = P360BusinessLogic.GetRunResultFromJson(existingState);
        else runResult = new RunResult();

        if (runResult.Steps.Any() && runResult.Steps.All(s => s.Success)) return;

        try
        {
            if (runResult.Steps.Any())
            {
                await P360BusinessLogic.Run(runResult);
            }
            else
            {
                await P360BusinessLogic.RunUploadFileToPrivatePerson(runResult, employee.SocialSecurityNumber, employee.FirstName, null, employee.LastName, employee.Address, employee.Zipcode, employee.City, employee.PhoneNumber, employee.Email, contractFile);
            }
            await Context.Reporter.Report($"{nameof(contract.SequenceNumber)}:{contract.SequenceNumber};{nameof(contract.ContractId)}:{contract.ContractId};{nameof(contract.EmployeeId)}:{contract.EmployeeId};{runResult}");
        }
        finally
        {
            var json = P360BusinessLogic.GetJsonFromRunResult(runResult);
            await Context.StateFileHandler.SaveState(contract, json);
        }
    }
}
