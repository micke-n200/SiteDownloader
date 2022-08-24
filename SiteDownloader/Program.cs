using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using SiteDownloader.Services;

namespace SiteDownloader;

internal sealed class Program
{
    private static async Task Main(string[] args)
    {
        await Host.CreateDefaultBuilder(args)
            .ConfigureServices((hostContext, services) =>
            {
                services.AddHostedService<ConsoleHostedService>();
                services.AddSingleton<IDownloadService, DownloadService>();
                services.AddHttpClient();
            })
            .UseConsoleLifetime()
            .RunConsoleAsync();
    }
}