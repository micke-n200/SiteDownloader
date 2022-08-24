using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using NSubstitute;
using SiteDownloader.Services;

namespace SiteDownloader.Tests
{
    public class DownloadServiceTests
    {
        [Fact]
        public async Task DownloadFile_UrlStartsWithHttp()
        {
            //Arrange
            var configuration = Substitute.For<IConfiguration>();
            var logger = new NullLogger<DownloadService>();
            var httpFactory = Substitute.For<IHttpClientFactory>();
            var client = Substitute.For<HttpClient>();
            var cancellationToken = new CancellationToken();
            var downloadService = new DownloadService(configuration, logger, httpFactory);
 
            //Act
            var result = await downloadService.DownloadFile(client, "http:\\www.google.com", true, cancellationToken);

            //Assert
            Assert.Null(result);
        }
    }
}