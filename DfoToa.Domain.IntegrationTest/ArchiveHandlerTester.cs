using Moq;
using P360Client;

namespace DfoToa.Domain.IntegrationTest
{
    public partial class ArchiveHandlerTester
    {
        private readonly ArchiveHandler _archiveHandler;
        private readonly Mock<IContext> _mockContext = new Mock<IContext>();
        private const string _inProductionDate = "2021-01-01";
        private const string _maskinportenScope = "dfo:ansatte dfo:stillinger dfo:organisasjoner dfo:ansatte/infokontrakter dfo:infokontrakter/filer";
        private const string _maskinportenAudience = "https://maskinporten.no/";
        private const string _maskinportenTokenEndpoint = "https://maskinporten.no/token";
        private const string _dfoApiBaseAddress = "https://api.dfo.no/";
        private const string _p360BaseAddress = "";

        public ArchiveHandlerTester()
        {
            var mockLog = new Mock<ILog>();
            _mockContext.Setup(c => c.CurrentLogger).Returns(mockLog.Object);
            _mockContext.Setup(c => c.Reporter).Returns(new Mock<IReport>().Object);
            _mockContext.Setup(c => c.StateFileHandler).Returns(new DStepFileHandler("state"));
            _mockContext.Setup(c => c.InProductionDate).Returns(_inProductionDate);
            _mockContext.Setup(c => c.SearchDate).Returns(new DateTime(2021,1,1));
            _mockContext.Setup(c => c.MaskinportenScope).Returns(_maskinportenScope);
            _mockContext.Setup(c => c.MaskinportenAudience).Returns(_maskinportenAudience);
            _mockContext.Setup(c => c.MaskinportenIssuer).Returns(_maskinportenIssuer);
            _mockContext.Setup(c => c.MaskinportenTokenEndpoint).Returns(_maskinportenTokenEndpoint);
            _mockContext.Setup(c => c.MaskinportenCertificatePath).Returns(_maskinportenCertificatePath);
            _mockContext.Setup(c => c.MaskinportenCertificatePassword).Returns(_maskinportenCertificatePassword);
            _mockContext.Setup(c => c.DfoApiBaseAddress).Returns(_dfoApiBaseAddress);
            _mockContext.Setup(c => c.BaseAddress).Returns(_p360BaseAddress);
            _mockContext.Setup(c => c.ApiKey).Returns(_p360ApiKey);
            _archiveHandler = new ArchiveHandler(_mockContext.Object);
        }

        [Fact]
        public async Task Archive_called__()
        {
            await _archiveHandler.Archive(new P360EmployeeContractHandler(_mockContext.Object), new List<string> { "" });
        }
    }
}
