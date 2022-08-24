namespace SiteDownloader.Services
{
    internal interface IDownloadService
    {
        Task GetFiles(CancellationToken cancellationToken);
    }
}
