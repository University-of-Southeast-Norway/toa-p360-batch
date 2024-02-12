using Moq;

namespace DfoToa.Archive.IntegrationTest
{
    public abstract class TesterBase
    {
        private string _apiKey = "<insert api key before testing>";

        protected string ApiKey { get => _apiKey == "<insert api key before testing>" ? throw new Exception($"Please insert api-key value in {nameof(TesterBase)}.{nameof(_apiKey)} before running integration-tests.") : _apiKey; }
        protected string BaseAddress { get; } = "https://usn-test.public360online.com";
        protected Mock<IContext> MockContext { get; } = new Mock<IContext>();

        public TesterBase()
        {
            MockContext.Setup(c => c.BaseAddress).Returns(BaseAddress);
            MockContext.Setup(c => c.ApiKey).Returns($"{ApiKey}");
        }
    }
}
