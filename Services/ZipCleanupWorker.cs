using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace comicsbox.Services;

public class ZipCleanupWorker : BackgroundService
{
    private readonly ILogger<ZipCleanupWorker> _logger;
    private readonly string _zipCacheDir;

    // ⏱ seuil d'expiration
    private static readonly TimeSpan MaxZipAge = TimeSpan.FromHours(4);

    // ⏲ fréquence de nettoyage
    private static readonly TimeSpan CleanupInterval = TimeSpan.FromMinutes(30);

    public ZipCleanupWorker(ILogger<ZipCleanupWorker> logger)
    {
        _logger = logger;
        _zipCacheDir = Path.Combine(
            Directory.GetCurrentDirectory(),
            "wwwroot",
            "cache",
            "zip"
        );
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("ZipCleanupWorker started");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                CleanupOldZips();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during ZIP cleanup");
            }

            try
            {
                await Task.Delay(CleanupInterval, stoppingToken);
            }
            catch (TaskCanceledException)
            {
                // normal à l'arrêt
            }
        }

        _logger.LogInformation("ZipCleanupWorker stopped");
    }

    private void CleanupOldZips()
    {
        if (!Directory.Exists(_zipCacheDir))
            return;

        var now = DateTime.UtcNow;

        foreach (var file in Directory.GetFiles(_zipCacheDir, "*.zip"))
        {
            try
            {
                var info = new FileInfo(file);
                var age = now - info.CreationTimeUtc;

                if (age > MaxZipAge)
                {
                    info.Delete();
                    _logger.LogInformation(
                        "Deleted old ZIP: {File} (age: {Hours}h)",
                        info.Name,
                        Math.Round(age.TotalHours, 1)
                    );
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(
                    ex,
                    "Failed to delete ZIP {File}",
                    Path.GetFileName(file)
                );
            }
        }
    }
}
