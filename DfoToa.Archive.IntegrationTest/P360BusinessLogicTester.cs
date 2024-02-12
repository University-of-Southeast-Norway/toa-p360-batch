using Moq;
using P360Client;
using System.Text;

namespace DfoToa.Archive.IntegrationTest
{
    public class P360BusinessLogicTester : TesterBase
    {
        [Fact]
        public async Task Run_called__Existing_person()
        {
            // Given
            MockContext.Setup(c => c.InProductionDate).Returns("2022-01-01");
            MockContext.Setup(c => c.CurrentLogger).Returns(new Mock<ILog>().Object);
            P360BusinessLogic.Init(MockContext.Object);

            string personlIDNumber = "11235813217";
            string firstName = "Pål";
            string middleName = null;
            string lastName = "Egg";
            string streetAddress = "Ribberullveien 1";
            string zipCode = "3110";
            string zipPlace = "Tønsberg";
            string mobilePhoneNumber = "11111111";
            string email = "paalegg@broedski.ve";
            DocumentService.Files2 fileInput = new DocumentService.Files2();
            fileInput.Title = "testFile.pdf";
            fileInput.Note = "00CE84DAECD9C419D8173BCBF9372160";
            fileInput.Format = "txt";
            var fileContentBase64 = File.ReadAllText(@"Data\ContractFileContent.txt");
            var plainTextBytes = Encoding.UTF8.GetBytes(fileContentBase64);
            fileInput.Base64Data = Convert.ToBase64String(plainTextBytes);

            // When
            RunResult runResult = new();
            await P360BusinessLogic.RunUploadFileToPrivatePerson(runResult, personlIDNumber, firstName, middleName, lastName, streetAddress, zipCode, zipPlace, mobilePhoneNumber, email, fileInput);
        }

        [Fact]
        public async Task Run_called__New_person()
        {
            // Given
            MockContext.Setup(c => c.InProductionDate).Returns("2022-01-01");
            MockContext.Setup(c => c.CurrentLogger).Returns(new Mock<ILog>().Object);
            P360BusinessLogic.Init(MockContext.Object);

            string personlIDNumber = $"{Random.Shared.Next(100000000, 999999999)}00";
            string firstName = "Pål";
            string middleName = null;
            string lastName = "Egg";
            string streetAddress = "Ribberullveien 1";
            string zipCode = "3110";
            string zipPlace = "Tønsberg";
            string mobilePhoneNumber = "11111111";
            string email = "paalegg@broedski.ve";
            DocumentService.Files2 fileInput = new DocumentService.Files2();
            fileInput.Title = "testFile.pdf";
            fileInput.Format = "txt";
            var fileContentBase64 = File.ReadAllText(@"Data\ContractFileContent.txt");
            var plainTextBytes = Encoding.UTF8.GetBytes(fileContentBase64);
            fileInput.Base64Data = Convert.ToBase64String(plainTextBytes);

            // When
            RunResult runResult = new();
            await P360BusinessLogic.RunUploadFileToPrivatePerson(runResult, personlIDNumber, firstName, middleName, lastName, streetAddress, zipCode, zipPlace, mobilePhoneNumber, email, fileInput);
        }
    }
}
