using comicsbox.Models;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Collections.Concurrent;
using System.IO.Compression;

namespace comicsbox.Services;

public class ZipWorker : BackgroundService
{
    private readonly ILogger<ZipWorker> _logger;
    private readonly List<BookCategory> _categories;

    // Queue thread-safe
    private readonly ConcurrentQueue<(string category, string series)> _queue = new();

    public ZipWorker(
        ILogger<ZipWorker> logger,
        IOptions<List<BookCategory>> categories)
    {
        _logger = logger;
        _categories = categories.Value;
    }

    /// <summary>
    /// Ajoute une série à la file de génération ZIP
    /// </summary>
    public void Enqueue(string category, string series)
    {
        if (string.IsNullOrWhiteSpace(category) || string.IsNullOrWhiteSpace(series))
            return;

        _logger.LogInformation(
            "Series queued for ZIP: {Category} / {Series}",
            category, series);

        _queue.Enqueue((category, series));
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            if (_queue.TryDequeue(out var item))
            {
                try
                {
                    await GenerateZipAsync(item.category, item.series, stoppingToken);
                }
                catch (OperationCanceledException)
                {
                    // arrêt normal
                }
                catch (Exception ex)
                {
                    _logger.LogError(
                        ex,
                        "Unhandled error generating ZIP for {Category}/{Series}",
                        item.category, item.series);
                }
            }
            else
            {
                await Task.Delay(1000, stoppingToken);
            }
        }
    }

    private async Task GenerateZipAsync(
        string category,
        string series,
        CancellationToken ct)
    {
        // Résolution de la catégorie
        var cat = _categories.FirstOrDefault(c =>
            string.Equals(c.Name, category, StringComparison.OrdinalIgnoreCase));

        if (cat == null || !Directory.Exists(cat.Path))
        {
            _logger.LogWarning("Category not found or invalid: {Category}", category);
            return;
        }

        var seriesPath = Path.Combine(cat.Path, series);
        if (!Directory.Exists(seriesPath))
        {
            _logger.LogWarning("Series folder not found: {SeriesPath}", seriesPath);
            return;
        }

        var pdfFiles = Directory
            .GetFiles(seriesPath, "*.pdf")
            .OrderBy(f => f)
            .ToList();

        if (!pdfFiles.Any())
        {
            _logger.LogWarning("No PDF files found: {SeriesPath}", seriesPath);
            return;
        }

        // Dossier cache ZIP
        var zipDir = Path.Combine(
            Directory.GetCurrentDirectory(),
            "wwwroot", "cache", "zip");

        Directory.CreateDirectory(zipDir);

        var zipFinalPath = Path.Combine(zipDir, $"{category}-{series}.zip");
        var zipTempPath  = zipFinalPath + ".tmp";

        // ZIP déjà généré
        if (File.Exists(zipFinalPath))
        {
            _logger.LogInformation("ZIP already exists: {Zip}", zipFinalPath);
            return;
        }

        // Nettoyage d'un .tmp précédent
        if (File.Exists(zipTempPath))
            File.Delete(zipTempPath);

        _logger.LogInformation(
            "Generating ZIP (temp): {Category} / {Series}",
            category, series);

        try
        {
            using (var zipStream = new FileStream(
                zipTempPath,
                FileMode.Create,
                FileAccess.Write,
                FileShare.None))
            using (var archive = new ZipArchive(zipStream, ZipArchiveMode.Create))
            {
                foreach (var filePath in pdfFiles)
                {
                    ct.ThrowIfCancellationRequested();

                    var safeFileName = Path.GetFileName(filePath);
                    var entryName = $"{series} - {safeFileName}";

                    archive.CreateEntryFromFile(
                        filePath,
                        entryName,
                        CompressionLevel.Fastest);
                }
            }

            // Move atomique vers le nom final
            File.Move(zipTempPath, zipFinalPath, overwrite: true);

            _logger.LogInformation("ZIP created successfully: {Zip}", zipFinalPath);
        }
        catch
        {
            // Nettoyage en cas d’erreur
            if (File.Exists(zipTempPath))
                File.Delete(zipTempPath);

            throw;
        }
    }
}
