using P360Client.DTO;
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


    protected override async Task ExecuteStep(ResourceClient client)
    {
        if (PersonlIDNumber == null) throw new Exception($"{nameof(PersonlIDNumber)} is null. Consider calling {nameof(SynchronizePersonStep)}({typeof(ResourceClient)},{typeof(string)}...) if this is the first step.");
        var synchronizePrivatePersonArgs = JsonDeserializerObsolete.GetSynchronizePrivatePersonArgs();
        synchronizePrivatePersonArgs.PersonalIdNumber = PersonlIDNumber;
        synchronizePrivatePersonArgs.FirstName = FirstName;
        synchronizePrivatePersonArgs.MiddleName = MiddleName;
        synchronizePrivatePersonArgs.LastName = LastName;
        synchronizePrivatePersonArgs.PrivateAddress = new Address
        {
            StreetAddress = StreetAddress,
            ZipCode = ZipCode,
            ZipPlace = ZipPlace,
            Country = "NOR"
        };
        synchronizePrivatePersonArgs.MobilePhone = MobilePhoneNumber;
        synchronizePrivatePersonArgs.Email = Email;
        Recno = await client.ContactResources.SynchronizePrivatePersonAsync(synchronizePrivatePersonArgs);
    }

    protected override async Task ExecuteStep<TStep>(ResourceClient client, TStep fromStep)
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