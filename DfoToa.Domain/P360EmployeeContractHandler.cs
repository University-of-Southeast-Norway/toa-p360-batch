﻿using DfoClient;
using DfoClient.Dto;
using DfoToa.Archive;
using P360Client.Domain;
using P360Client.DTO;

namespace DfoToa.Domain;

public class P360EmployeeContractHandler : IEmployeeContractHandler
{
    public IContext Context { get; set; }

    public P360EmployeeContractHandler(IContext context)
    {
        Context = context;
    }

    public async Task RunAsync(Employee employee, Contract contract, EmployeeContract employeeContract, Employee? caseManager)
    {
        UniqueTitlesTemplate? template = await Context.MappingTemplates.GetTitlesTemplate();
        string title = $"Signert avtale {contract.SequenceNumber}";
        if (!string.IsNullOrEmpty(template?.File))
        {
            title = template.File.Replace("{contract.sequenceNumber}", contract.SequenceNumber)
                .Replace("{contract.contractNumber}", contract.ContractId)
                .Replace("{employeeContract.startDate}", employeeContract.StartDate.ToString("dd.MM.yyyy"))
                .Replace("{employeeContract.endDate}", employeeContract.EndDate.ToString("dd.MM.yyyy"));
        }
        NewDocumentFile contractFile = new()
        {
            Title = title,
            Format = "pdf",
            Note = Utility.CreateChecksum(contract.FileContent),
            Base64Data = contract.FileContent
        };
        P360BusinessLogic.Init(Context);
        string? existingState = await Context.StateFileHandler.GetState(contract);
        RunResult? runResult = null;

        if (!string.IsNullOrEmpty(existingState)) runResult = P360BusinessLogic.GetRunResultFromJson(existingState);
        else runResult = new();

        if (runResult.Steps.Any() && runResult.Steps.All(s => s.Success)) return;

        try
        {
            if (runResult.Steps.Any())
            {
                await P360BusinessLogic.Run(runResult);
            }
            else
            {
                UniqueQueryAttributesTemplate? responsibleTemplate = await Context.MappingTemplates.GetResponsibleTemplate();
                string? externalId = responsibleTemplate?.ExternalId;
                string? email = responsibleTemplate?.Email;
                PrivatePerson? responsiblePerson = null;
                if (responsibleTemplate is not null && caseManager is not null)
                {
                    responsiblePerson = new()
                    {
                        ExternalID = externalId?.Replace("{dfo.caseManager.id}", caseManager.Id)?.Replace("{dfo.caseManager.email}", caseManager.Email),
                        Email = email?.Replace("{dfo.caseManager.id}", caseManager.Id)?.Replace("{dfo.caseManager.email}", caseManager.Email)
                    };
                }
                await P360BusinessLogic.RunUploadFileToPrivatePerson(runResult, employee.SocialSecurityNumber, employee.FirstName, null, employee.LastName, employee.Address, employee.Zipcode, employee.City, employee.PhoneNumber, employee.Email, contractFile, contract.Date, responsiblePerson, null);
            }
            await Context.Reporter.Report($"{nameof(contract.SequenceNumber)}:{contract.SequenceNumber};{nameof(contract.ContractId)}:{contract.ContractId};{nameof(contract.EmployeeId)}:{contract.EmployeeId};{runResult}");
        }
        finally
        {
            string json = P360BusinessLogic.GetJsonFromRunResult(runResult);
            await Context.StateFileHandler.SaveState(contract, json);
        }
    }
}
