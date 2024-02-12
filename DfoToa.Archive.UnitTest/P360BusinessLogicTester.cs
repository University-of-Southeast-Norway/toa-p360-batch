using DfoToa.Archive.Steps;
using Moq;
using P360Client;

namespace DfoToa.Archive.UnitTest
{
    [Collection("P360BusinessLogicTester")]
    public class P360BusinessLogicTester
    {
        private Mock<IContext> _mockContext;
        private Mock<CaseService.ICaseServiceClient> _mockCaseService;
        private Mock<ContactService.IContactServiceClient> _mockContactService;
        private Mock<DocumentService.IDocumentServiceClient> _mockDocumentService;
        private Mock<FileService.IFileServiceClient> _mockFileService;
        private Mock<ProjectService.IProjectServiceClient> _mockProjectService;
        private Mock<SupportService.ISupportServiceClient> _mockSupportService;
        private Client _client;
        DateTime inProductionDate = DateTime.Now.Date;

        public P360BusinessLogicTester()
        {
            Mock<ILog> mockLog = new();
            _mockContext = new();
            _mockContext.Setup(c => c.CurrentLogger).Returns(mockLog.Object);
            _mockContext.Setup(c => c.BaseAddress).Returns(@"http://localhost");
            _mockContext.Setup(c => c.ApiKey).Returns(@"P360ApiKey");
            _mockContext.Setup(c => c.InProductionDate).Returns(inProductionDate.ToString());
            _mockCaseService = new();
            _mockContactService = new();
            _mockDocumentService = new();
            _mockFileService = new();
            _mockProjectService = new();
            _mockSupportService = new();
            _client = new Client(_mockContext.Object, _mockCaseService.Object, _mockContactService.Object,
                _mockDocumentService.Object, _mockFileService.Object, _mockProjectService.Object, _mockSupportService.Object);
            P360BusinessLogic.Init(_mockContext.Object, _client);
        }

        [Fact]
        public async Task Run_called__GetPrivatePersonsAsync_returns_two_PrivatePersons__RunResult_Steps_is_empty()
        {
            // Given
            _mockContactService.Setup(c => c.GetPrivatePersonsAsync(It.IsAny<ContactService.GetPrivatePersonsArgs>())).Returns(Task.FromResult(new ContactService.Response5
            {
                PrivatePersons = new List<ContactService.PrivatePersons> { new ContactService.PrivatePersons(), new ContactService.PrivatePersons() } // Found 2 PrivatePersons.
            }));
            string personalIdNumber = "personalIdNumber";
            string firstName = "firstName";
            string middleName = "middleName";
            string lastName = "lastName";
            string streetAddress = "streetAddress";
            string zipCode = "zipCode";
            string zipPlace = "zipPlace";
            string mobilePhoneNumber = "mobilePhoneNumber";
            string email = "email";
            DocumentService.Files2 fileInput = new DocumentService.Files2();

            // When
            RunResult runResult = new();
            await P360BusinessLogic.RunUploadFileToPrivatePerson(runResult, personalIdNumber, firstName, middleName, lastName, streetAddress, zipCode, zipPlace, mobilePhoneNumber, email, fileInput);

            // Then
            Assert.Empty(runResult.Steps);
        }

        [Fact]
        public async Task Run_called__GetPrivatePersonsAsync_returns_one_PrivatePersons_recno_not_given__Throws_exception()
        {
            // Given
            _mockContactService.Setup(c => c.GetPrivatePersonsAsync(It.IsAny<ContactService.GetPrivatePersonsArgs>())).Returns(Task.FromResult(new ContactService.Response5
            {
                PrivatePersons = new List<ContactService.PrivatePersons> { new ContactService.PrivatePersons() } // Found one PrivatePersons without recno.
            }));
            string personalIdNumber = "personalIdNumber";
            string firstName = "firstName";
            string middleName = "middleName";
            string lastName = "lastName";
            string streetAddress = "streetAddress";
            string zipCode = "zipCode";
            string zipPlace = "zipPlace";
            string mobilePhoneNumber = "mobilePhoneNumber";
            string email = "email";
            DocumentService.Files2 fileInput = new DocumentService.Files2();

            // When
            async Task task() => await P360BusinessLogic.RunUploadFileToPrivatePerson(new RunResult(), personalIdNumber, firstName, middleName, lastName, streetAddress, zipCode, zipPlace, mobilePhoneNumber, email, fileInput);
            var ex = await Assert.ThrowsAsync<Exception>(task);

            // Then
            _mockContactService.Verify(c => c.GetPrivatePersonsAsync(It.IsAny<ContactService.GetPrivatePersonsArgs>()), Times.Once);
            Assert.Equal("Recno is null. This property is required to have value.", ex.Message);
        }

        [Fact]
        public async Task Run_called__GetPrivatePersonsAsync_returns_one_PrivatePersons__RunResult_returns_asserted_steps()
        {
            // Given
            const string CaseNumber = "CaseNumber";
            const string DocumentNumber = "DocumentNumber";
            const string PersonalIdNumber = "personalIdNumber";
            const string FirstName = "firstName";
            const string MiddleName = "middleName";
            const string LastName = "lastName";
            const string StreetAddress = "streetAddress";
            const string ZipCode = "zipCode";
            const string ZipPlace = "zipPlace";
            const string MobilePhoneNumber = "mobilePhoneNumber";
            const string Email = "email";

            DocumentService.Files2 fileInput = new();

            _mockContactService.Setup(c => c.GetPrivatePersonsAsync(It.IsAny<ContactService.GetPrivatePersonsArgs>())).Returns(Task.FromResult(new ContactService.Response5
            {
                PrivatePersons = new List<ContactService.PrivatePersons> { new ContactService.PrivatePersons { Recno = 1 } } // Found one PrivatePerson with recno.
            }));
            _mockCaseService.Setup(c => c.GetCasesAsync(It.IsAny<CaseService.GetCasesArgs>())).Returns(Task.FromResult(new CaseService.Response2
            {
                Cases = new List<CaseService.Cases> {
                    new CaseService.Cases
                    {
                        CaseNumber = CaseNumber,
                        Status = "Under behandling",
                        CreatedDate = inProductionDate.AddDays(1)
                    } } // Found one case, in progress, created after InProductionDate.
            }));

            DocumentService.CreateDocumentArgs? createDocumentResult = null;
            _mockDocumentService.Setup(d => d.CreateDocumentAsync(It.IsAny<DocumentService.CreateDocumentArgs>()))
                .Callback<DocumentService.CreateDocumentArgs>(r => createDocumentResult = r)
                .Returns(Task.FromResult(new DocumentService.Response { DocumentNumber = DocumentNumber })); // Creates document.

            DocumentService.UpdateDocumentArgs? updateDocumentArgs = null;
            _mockDocumentService.Setup(d => d.UpdateDocumentAsync(It.IsAny<DocumentService.UpdateDocumentArgs>())).Callback<DocumentService.UpdateDocumentArgs>(u => updateDocumentArgs = u);

            // When
            RunResult runResult = new();
            await P360BusinessLogic.RunUploadFileToPrivatePerson(runResult, PersonalIdNumber, FirstName, MiddleName, LastName, StreetAddress, ZipCode, ZipPlace, MobilePhoneNumber, Email, fileInput);

            // Then
            _mockContactService.Verify(c => c.GetPrivatePersonsAsync(It.IsAny<ContactService.GetPrivatePersonsArgs>()), Times.Once);
            _mockCaseService.Verify(c => c.GetCasesAsync(It.IsAny<CaseService.GetCasesArgs>()), Times.Once);
            Assert.DoesNotContain(runResult.Steps, s => s is CreateCaseStep);
            Assert.Single(runResult.Steps, s => s is CreateDocumentStep);
            Assert.Single(runResult.Steps, s => s is UpdateDocumentWithFileReferenceStep);
            Assert.Single(runResult.Steps, s => s is SignOffDocumentStep);
            Assert.Equal(CaseNumber, createDocumentResult?.Parameter.CaseNumber);
            Assert.Equal(DocumentNumber, updateDocumentArgs?.Parameter.DocumentNumber);
        }

        [Fact]
        public async Task Run_called__GetPrivatePersonsAsync_returns_one_PrivatePersons_but_no_case_found__RunResult_returns_asserted_steps()
        {
            // Given
            const string CaseNumber = "CaseNumber";
            const string DocumentNumber = "DocumentNumber";
            const string PersonalIdNumber = "personalIdNumber";
            const string FirstName = "firstName";
            const string MiddleName = "middleName";
            const string LastName = "lastName";
            const string StreetAddress = "streetAddress";
            const string ZipCode = "zipCode";
            const string ZipPlace = "zipPlace";
            const string MobilePhoneNumber = "mobilePhoneNumber";
            const string Email = "email";
            const int Recno = 1;

            DocumentService.Files2 fileInput = new();

            _mockContactService.Setup(c => c.GetPrivatePersonsAsync(It.IsAny<ContactService.GetPrivatePersonsArgs>())).Returns(Task.FromResult(new ContactService.Response5
            {
                PrivatePersons = new List<ContactService.PrivatePersons> { new ContactService.PrivatePersons { Recno = Recno } } // Found one PrivatePerson with recno.
            }));
            _mockCaseService.Setup(c => c.GetCasesAsync(It.IsAny<CaseService.GetCasesArgs>())).Returns(Task.FromResult(new CaseService.Response2
            {
                Cases = new List<CaseService.Cases>() // No cases found.
            }));

            CaseService.CreateCaseArgs? createCaseArgs = null;
            _mockCaseService.Setup(c => c.CreateCaseAsync(It.IsAny<CaseService.CreateCaseArgs>()))
                .Callback<CaseService.CreateCaseArgs>(c => createCaseArgs = c)
                .Returns(Task.FromResult(new CaseService.Response { CaseNumber = CaseNumber })); // Case created.

            DocumentService.CreateDocumentArgs? createDocumentResult = null;
            _mockDocumentService.Setup(d => d.CreateDocumentAsync(It.IsAny<DocumentService.CreateDocumentArgs>()))
                .Callback<DocumentService.CreateDocumentArgs>(r => createDocumentResult = r)
                .Returns(Task.FromResult(new DocumentService.Response { DocumentNumber = DocumentNumber })); // Creates document.

            DocumentService.UpdateDocumentArgs? updateDocumentArgs = null;
            _mockDocumentService.Setup(d => d.UpdateDocumentAsync(It.IsAny<DocumentService.UpdateDocumentArgs>())).Callback<DocumentService.UpdateDocumentArgs>(u => updateDocumentArgs = u);

            // When
            RunResult runResult = new();
            await P360BusinessLogic.RunUploadFileToPrivatePerson(runResult, PersonalIdNumber, FirstName, MiddleName, LastName, StreetAddress, ZipCode, ZipPlace, MobilePhoneNumber, Email, fileInput);

            // Then
            _mockContactService.Verify(c => c.GetPrivatePersonsAsync(It.IsAny<ContactService.GetPrivatePersonsArgs>()), Times.Once);
            _mockCaseService.Verify(c => c.GetCasesAsync(It.IsAny<CaseService.GetCasesArgs>()), Times.Once);
            Assert.Single(runResult.Steps, s => s is CreateCaseStep);
            Assert.Single(runResult.Steps, s => s is CreateDocumentStep);
            Assert.Single(runResult.Steps, s => s is UpdateDocumentWithFileReferenceStep);
            Assert.Single(runResult.Steps, s => s is SignOffDocumentStep);
            Assert.Equal($"recno:{Recno}", createCaseArgs?.Parameter.Contacts.First().ReferenceNumber);
            Assert.Equal(CaseNumber, createDocumentResult?.Parameter.CaseNumber);
            Assert.Equal(DocumentNumber, updateDocumentArgs?.Parameter.DocumentNumber);
        }

        [Fact]
        public async Task Run_called__GetPrivatePersonsAsync_returns_zero_PrivatePersons__RunResult_returns_asserted_steps()
        {
            // Given
            const string CaseNumber = "CaseNumber";
            const string DocumentNumber = "DocumentNumber";
            const string PersonalIdNumber = "personalIdNumber";
            const string FirstName = "firstName";
            const string MiddleName = "middleName";
            const string LastName = "lastName";
            const string StreetAddress = "streetAddress";
            const string ZipCode = "zipCode";
            const string ZipPlace = "zipPlace";
            const string MobilePhoneNumber = "mobilePhoneNumber";
            const string Email = "email";
            const int Recno = 1;

            DocumentService.Files2 fileInput = new();

            _mockContactService.Setup(c => c.GetPrivatePersonsAsync(It.IsAny<ContactService.GetPrivatePersonsArgs>())).Returns(Task.FromResult(new ContactService.Response5
            {
                PrivatePersons = new List<ContactService.PrivatePersons>() // Found zero PrivatePerson.
            }));

            ContactService.SynchronizePrivatePersonArgs? synchronizePrivatePersonArgs = null;
            _mockContactService.Setup(c => c.SynchronizePrivatePersonAsync(It.IsAny<ContactService.SynchronizePrivatePersonArgs>()))
                .Callback<ContactService.SynchronizePrivatePersonArgs>(s => synchronizePrivatePersonArgs = s)
                .Returns(Task.FromResult(new ContactService.Response2 { Recno = Recno }));

            CaseService.CreateCaseArgs? createCaseArgs = null;
            _mockCaseService.Setup(c => c.CreateCaseAsync(It.IsAny<CaseService.CreateCaseArgs>()))
                .Callback<CaseService.CreateCaseArgs>(c => createCaseArgs = c)
                .Returns(Task.FromResult(new CaseService.Response { CaseNumber = CaseNumber })); // Case created.

            DocumentService.CreateDocumentArgs? createDocumentResult = null;
            _mockDocumentService.Setup(d => d.CreateDocumentAsync(It.IsAny<DocumentService.CreateDocumentArgs>()))
                .Callback<DocumentService.CreateDocumentArgs>(r => createDocumentResult = r)
                .Returns(Task.FromResult(new DocumentService.Response { DocumentNumber = DocumentNumber })); // Creates document.

            DocumentService.UpdateDocumentArgs? updateDocumentArgs = null;
            _mockDocumentService.Setup(d => d.UpdateDocumentAsync(It.IsAny<DocumentService.UpdateDocumentArgs>())).Callback<DocumentService.UpdateDocumentArgs>(u => updateDocumentArgs = u);

            // When
            RunResult runResult = new();
            await P360BusinessLogic.RunUploadFileToPrivatePerson(runResult, PersonalIdNumber, FirstName, MiddleName, LastName, StreetAddress, ZipCode, ZipPlace, MobilePhoneNumber, Email, fileInput);

            // Then
            _mockContactService.Verify(c => c.GetPrivatePersonsAsync(It.IsAny<ContactService.GetPrivatePersonsArgs>()), Times.Once);
            Assert.Single(runResult.Steps, s => s is SynchronizePersonStep);
            Assert.Single(runResult.Steps, s => s is CreateCaseStep);
            Assert.Single(runResult.Steps, s => s is CreateDocumentStep);
            Assert.Single(runResult.Steps, s => s is UpdateDocumentWithFileReferenceStep);
            Assert.Single(runResult.Steps, s => s is SignOffDocumentStep);
            Assert.Equal(PersonalIdNumber, synchronizePrivatePersonArgs?.Parameter.PersonalIdNumber);
            Assert.Equal($"recno:{Recno}", createCaseArgs?.Parameter.Contacts.First().ReferenceNumber);
            Assert.Equal(CaseNumber, createDocumentResult?.Parameter.CaseNumber);
            Assert.Equal(DocumentNumber, updateDocumentArgs?.Parameter.DocumentNumber);
        }

        [Fact]
        public async Task Run_called__GetPrivatePersonsAsync_returns_zero_PrivatePersons__Json_test()
        {
            // Given
            const string CaseNumber = "CaseNumber";
            const string PersonalIdNumber = "personalIdNumber";
            const string FirstName = "firstName";
            const string MiddleName = "middleName";
            const string LastName = "lastName";
            const string StreetAddress = "streetAddress";
            const string ZipCode = "zipCode";
            const string ZipPlace = "zipPlace";
            const string MobilePhoneNumber = "mobilePhoneNumber";
            const string Email = "email";
            const int Recno = 1;

            DocumentService.Files2 fileInput = new();

            _mockContactService.Setup(c => c.GetPrivatePersonsAsync(It.IsAny<ContactService.GetPrivatePersonsArgs>())).Returns(Task.FromResult(new ContactService.Response5
            {
                PrivatePersons = new List<ContactService.PrivatePersons>() // Found zero PrivatePerson.
            }));

            ContactService.SynchronizePrivatePersonArgs? synchronizePrivatePersonArgs = null;
            _mockContactService.Setup(c => c.SynchronizePrivatePersonAsync(It.IsAny<ContactService.SynchronizePrivatePersonArgs>()))
                .Callback<ContactService.SynchronizePrivatePersonArgs>(s => synchronizePrivatePersonArgs = s)
                .Returns(Task.FromResult(new ContactService.Response2 { Recno = Recno }));

            CaseService.CreateCaseArgs? createCaseArgs = null;
            _mockCaseService.Setup(c => c.CreateCaseAsync(It.IsAny<CaseService.CreateCaseArgs>()))
                .Callback<CaseService.CreateCaseArgs>(c => createCaseArgs = c)
                .Returns(Task.FromResult(new CaseService.Response { CaseNumber = CaseNumber })); // Case created.

            _mockDocumentService.Setup(d => d.CreateDocumentAsync(It.IsAny<DocumentService.CreateDocumentArgs>())).Throws<Exception>(); // Create document throws exception.

            // When
            RunResult runResult = new RunResult();
            try
            {
                await P360BusinessLogic.RunUploadFileToPrivatePerson(runResult, PersonalIdNumber, FirstName, MiddleName, LastName, StreetAddress, ZipCode, ZipPlace, MobilePhoneNumber, Email, fileInput);
            }
            catch { } // Create document will fail.
            _mockDocumentService.Reset(); // Fixes the error with create document.

            var json = P360BusinessLogic.GetJsonFromRunResult(runResult);

            runResult = P360BusinessLogic.GetRunResultFromJson(json);
            await P360BusinessLogic.Run(runResult);
            json = P360BusinessLogic.GetJsonFromRunResult(runResult);

            // Then

        }
    }
}