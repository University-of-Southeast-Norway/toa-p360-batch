using P360Client;
using static DfoToa.Archive.Steps.CreateCaseStep;
using static DfoToa.Archive.Steps.TryFindCaseStep;

namespace DfoToa.Archive.Steps;

public class SynchronizePersonStep : Step, IHaveCreateCaseStepDependencies, IHaveTryFindCaseStepDependencies
{
    public interface IHaveSynchronizePersonStepDependencies : IStep
    {
        string PersonlIdNumber { get; }
    }
    public string PersonlIDNumber { get; set; }
    public string FirstName { get; set; }
    public string MiddleName { get; set; }
    public string LastName { get; set; }
    public string StreetAddress { get; set; }
    public string ZipCode { get; set; }
    public string ZipPlace { get; set; }
    public string MobilePhoneNumber { get; set; }
    public string Email { get; set; }

    public int? Recno { get; set; }

    internal SynchronizePersonStep(string personlIDNumber, string firstName, string middleName, string lastName, string streetAddress, string zipCode, string zipPlace, string mobilePhoneNumber, string email)
        : this(firstName, middleName, lastName, streetAddress, zipCode, zipPlace, mobilePhoneNumber, email)
    {
        PersonlIDNumber = personlIDNumber;
    }

    internal SynchronizePersonStep(string firstName, string middleName, string lastName, string streetAddress, string zipCode, string zipPlace, string mobilePhoneNumber, string email) : this()
    {
        FirstName = firstName;
        MiddleName = middleName;
        LastName = lastName;
        StreetAddress = streetAddress;
        ZipCode = zipCode;
        ZipPlace = zipPlace;
        MobilePhoneNumber = mobilePhoneNumber;
        Email = email;
    }

    internal SynchronizePersonStep() : base(nameof(SynchronizePersonStep))
    {

    }


    protected override async Task ExecuteStep(Client client)
    {
        if (PersonlIDNumber == null) throw new Exception($"{nameof(PersonlIDNumber)} is null. Consider calling {nameof(SynchronizePersonStep)}({typeof(Client)},{typeof(string)}...) if this is the first step.");
        var synchronizePrivatePersonArgs = JsonDeserializerObsolete.GetSynchronizePrivatePersonArgs();
        synchronizePrivatePersonArgs.Parameter.PersonalIdNumber = PersonlIDNumber;
        synchronizePrivatePersonArgs.Parameter.FirstName = FirstName;
        synchronizePrivatePersonArgs.Parameter.MiddleName = MiddleName;
        synchronizePrivatePersonArgs.Parameter.LastName = LastName;
        synchronizePrivatePersonArgs.Parameter.PrivateAddress = new ContactService.PrivateAddress2
        {
            StreetAddress = StreetAddress,
            ZipCode = ZipCode,
            ZipPlace = ZipPlace,
            Country = "NOR"
        };
        synchronizePrivatePersonArgs.Parameter.MobilePhone = MobilePhoneNumber;
        synchronizePrivatePersonArgs.Parameter.Email = Email;
        Recno = await client.SynchronizePrivatePersonAsync(synchronizePrivatePersonArgs);
    }

    protected override async Task ExecuteStep<TStep>(Client client, TStep fromStep)
    {
        if (fromStep is IHaveSynchronizePersonStepDependencies step)
        {
            PersonlIDNumber = step.PersonlIdNumber;
            await ExecuteStep(client);
            return;
        }
        throw new Exception($"Can't execute from step {fromStep.GetType()}. '{nameof(fromStep)}' must implement {typeof(IHaveSynchronizePersonStepDependencies)}.");
    }

    public override string ToString()
    {
        return $"{nameof(SynchronizePersonStep)}:{nameof(Recno)}:{Recno}";
    }
}