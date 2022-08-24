using Microsoft.Extensions.Logging;
using HtmlAgilityPack;
using Microsoft.Extensions.Configuration;
using File = System.IO.File;

namespace SiteDownloader.Services
{
    internal class DownloadService : IDownloadService
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<DownloadService> _logger;
        private readonly IHttpClientFactory _httpFactory;
        private static int _downloadedFiles;

        public DownloadService(
            IConfiguration configuration,
            ILogger<DownloadService> logger,
            IHttpClientFactory httpFactory)
        {
            _configuration = configuration;
            _logger = logger;
            _httpFactory = httpFactory;
        }

        public async Task GetFiles(CancellationToken cancellationToken)
        {
            Console.WriteLine("Starting downloading site...");

            var client = _httpFactory.CreateClient();
            client.BaseAddress = new Uri(_configuration["Url"]);

            await GetPage(client, "/", cancellationToken);

            Console.WriteLine("Done.");
        }

        private async Task GetPage(HttpClient client, string url, CancellationToken cancellationToken)
        {
            var stream = await DownloadFile(client, url, true, cancellationToken);

            if (stream != null)
            {
                stream.Position = 0;

                var doc = new HtmlDocument();
                doc.Load(stream);

                var hrefList = doc.DocumentNode.SelectNodes("//a")?
                    .Select(p => p.GetAttributeValue("href", ""))
                    .Where(p => p != string.Empty)
                    .Distinct()
                    .ToList();

                var imageList = doc.DocumentNode.SelectNodes("//img")?
                    .Select(p => p.GetAttributeValue("src", ""))
                    .Where(p => p != string.Empty)
                    .Distinct()
                    .ToList();

                var cssList = doc.DocumentNode.SelectNodes("//link")?
                    .Select(p => p.GetAttributeValue("href", ""))
                    .Where(p => p != string.Empty)
                    .Distinct()
                    .ToList();

                var scriptList = doc.DocumentNode.SelectNodes("//script")?
                    .Select(p => p.GetAttributeValue("src", ""))
                    .Where(p => p != string.Empty)
                    .Distinct()
                    .ToList();

                await SaveFile(client, imageList, cancellationToken);
                await SaveFile(client, cssList, cancellationToken);
                await SaveFile(client, scriptList, cancellationToken);

                if (hrefList != null)
                {
                    foreach (var href in hrefList)
                    {
                        await GetPage(client, href, cancellationToken);
                    }
                }
            }
        }

        private async Task SaveFile(HttpClient client, List<string>? fileList, CancellationToken cancellationToken)
        {
            if (fileList == null)
                return;

            foreach (var fileUrl in fileList)
            {
                await DownloadFile(client, fileUrl, false, cancellationToken);
            }
        }

        internal async Task<Stream?> DownloadFile(HttpClient client, string url, bool isHtml, CancellationToken cancellationToken)
        {
            // Only download local files, no bookmark links, no javascript links, no mailto links and no tel links
            if (url.StartsWith("http") || url.StartsWith("//") || url.Contains("#") || url.StartsWith("javascript") || url.StartsWith("mailto") || url.StartsWith("tel"))
                return null;
            
            //Remove '?' since it's not valid in filename
            if (url.Contains('?'))
                url = url[..url.IndexOf('?')];
            
            //Remove path traversal and change url to local path
            url = url.Replace("../", "").Replace("/", "\\");
            var filePath = Path.Join(_configuration["DownloadFolder"], url);
            if (isHtml && !filePath.EndsWith(".html"))
                filePath = Path.Join(filePath, "index.html");

            if (File.Exists(filePath))
                return null;

            var folderPath = Path.GetDirectoryName(filePath);
            if (folderPath != null && !Directory.Exists(folderPath))
                Directory.CreateDirectory(folderPath);

            var response = await client.GetAsync(url, cancellationToken);

            Stream? result = null;
            if (response.IsSuccessStatusCode)
            {
                result = await response.Content.ReadAsStreamAsync(cancellationToken);

                await using (var fileStream = File.Create(filePath))
                {
                    await result.CopyToAsync(fileStream, cancellationToken);
                }

                Console.Write("\rFiles downloaded {0}...", ++_downloadedFiles);
            }
            else
            {
                _logger.LogError($"File could not be downloaded: {url}");
            }

            return result;
        }
    }
}
