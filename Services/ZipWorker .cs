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

    // Queue thread-safe pour les séries à zipper
    private readonly ConcurrentQueue<(string category, string series)> _queue = new();

    public ZipWorker(ILogger<ZipWorker> logger, IOptions<List<BookCategory>> categories)
    {
        _logger = logger;
        _categories = categories.Value;
    }

    /// <summary>
    /// Ajouter une série à la queue
    /// </summary>
    public void Enqueue(string category, string series)
    {
        if (string.IsNullOrWhiteSpace(category) || string.IsNullOrWhiteSpace(series))
            return;

        _logger.LogInformation("Series queued for ZIP: {Category} / {Series}", category, series);
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
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error generating ZIP for {Series}", item.series);
                }
            }
            else
            {
                await Task.Delay(1000, stoppingToken);
            }
        }
    }

    private async Task GenerateZipAsync(string category, string series, CancellationToken ct)
    {
        // Trouver la catégorie réelle
        var cat = _categories.FirstOrDefault(c => string.Equals(c.Name, category, StringComparison.OrdinalIgnoreCase));
        if (cat == null || !Directory.Exists(cat.Path))
        {
            _logger.LogWarning("Category not found or folder missing: {Category}", category);
            return;
        }

        // Vérifier le dossier série
        var seriesPath = Path.Combine(cat.Path, series);
        if (!Directory.Exists(seriesPath))
        {
            _logger.LogWarning("Series folder not found: {SeriesPath}", seriesPath);
            return;
        }

        // Chercher les fichiers PDF
        var pdfFiles = Directory.GetFiles(seriesPath, "*.pdf").OrderBy(f => f).ToList();
        if (!pdfFiles.Any())
        {
            _logger.LogWarning("No PDF files found in series: {SeriesPath}", seriesPath);
            return;
        }

        // Créer le dossier ZIP cache si nécessaire
        var zipDir = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "cache", "zip");
        if (!Directory.Exists(zipDir))
            Directory.CreateDirectory(zipDir);

        // Chemin final du ZIP
        var zipPath = Path.Combine(zipDir, $"{category}-{series}.zip");
        if (File.Exists(zipPath))
        {
            _logger.LogInformation("ZIP already exists: {ZipPath}", zipPath);
            return; // déjà créé
        }

        _logger.LogInformation("Generating ZIP: {Category} / {Series}", category, series);

        using var memoryStream = new MemoryStream();
        using (var archive = new ZipArchive(memoryStream, ZipArchiveMode.Create, true))
        {
            foreach (var filePath in pdfFiles)
            {
                ct.ThrowIfCancellationRequested();

                var safeFileName = Path.GetFileName(filePath); // empêche path traversal
                var entryName = $"{series} - {safeFileName}";    // renommage dans le ZIP

                archive.CreateEntryFromFile(filePath, entryName, CompressionLevel.Fastest);
            }
        }

        memoryStream.Seek(0, SeekOrigin.Begin);
        await File.WriteAllBytesAsync(zipPath, memoryStream.ToArray(), ct);

        _logger.LogInformation("ZIP created: {ZipPath}", zipPath);
    }
}
