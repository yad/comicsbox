using Microsoft.Extensions.Options;
using comicsbox.Models;

namespace comicsbox.Services;

public class ThumbnailWorker : BackgroundService
{
    private readonly List<BookCategory> _categories;
    private readonly ILogger<ThumbnailWorker> _logger;
    private readonly ImageService _imageService;
    private readonly PdfReaderService _pdfReaderService;
    private readonly string _cacheDir;

    public ThumbnailWorker(IOptions<List<BookCategory>> categories, ILogger<ThumbnailWorker> logger,
        ImageService imageService, PdfReaderService pdfReaderService)
    {
        _categories = categories.Value;
        _logger = logger;
        _imageService = imageService;
        _pdfReaderService = pdfReaderService;
        _cacheDir = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "cache", "thumbnails");
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("ThumbnailWorker starting thumbnail generation...");

        try
        {
            if (!Directory.Exists(_cacheDir))
            {
                Directory.CreateDirectory(_cacheDir);
            }

            foreach (var category in _categories)
            {
                if (stoppingToken.IsCancellationRequested)
                    break;

                if (!Directory.Exists(category.Path))
                {
                    _logger.LogWarning($"Category path not found: {category.Path}");
                    continue;
                }

                var seriesDirs = Directory.GetDirectories(category.Path);
                foreach (var seriesPath in seriesDirs)
                {
                    if (stoppingToken.IsCancellationRequested)
                        break;

                    var seriesName = Path.GetFileName(seriesPath);
                    await GenerateThumbnailAsync(category.Name, seriesName, seriesPath, stoppingToken);
                }
            }

            _logger.LogInformation("ThumbnailWorker completed thumbnail generation");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in ThumbnailWorker");
        }
    }

    private async Task GenerateThumbnailAsync(
        string categoryName,
        string seriesName,
        string seriesPath,
        CancellationToken stoppingToken)
    {
        try
        {
            var pdfs = Directory.GetFiles(seriesPath, "*.pdf")
                                .OrderBy(f => f)
                                .ToList();

            if (!pdfs.Any())
            {
                _logger.LogDebug($"No PDFs found in {seriesPath}");
                return;
            }

            foreach (var pdfPath in pdfs)
            {
                if (stoppingToken.IsCancellationRequested)
                    break;

                var fileInfo = new FileInfo(pdfPath);
                var thumbnailFileName = ThumbnailHelper.GetThumbnailFileName(pdfPath);
                var cachePath = Path.Combine(_cacheDir, thumbnailFileName);

                // Vérifie le cache
                if (File.Exists(cachePath))
                {
                    var thumbnailInfo = new FileInfo(cachePath);

                    if (fileInfo.LastWriteTimeUtc <= thumbnailInfo.LastWriteTimeUtc)
                    {
                        _logger.LogDebug($"Thumbnail up to date: {thumbnailFileName}");
                        continue;
                    }

                    _logger.LogInformation($"PDF newer than thumbnail, regenerating: {thumbnailFileName}");
                    File.Delete(cachePath);
                }

                await Task.Run(() => ProcessPdfThumbnail(fileInfo, cachePath), stoppingToken);
                _logger.LogInformation($"Generated thumbnail: {thumbnailFileName}");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error generating thumbnails for {categoryName}/{seriesName}");
        }
    }


    private void ProcessPdfThumbnail(FileInfo fileInfo, string cachePath)
    {
        try
        {
            var image = _pdfReaderService.LoadFile(fileInfo.FullName, false).ReadCoverImage();
            var thumbnailContent = _imageService.ScaleAsThumbnail(image);
            File.WriteAllBytes(cachePath, thumbnailContent);
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning($"{fileInfo.FullName}: Thumbnail cannot be found - {ex.Message}");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error processing PDF thumbnail: {fileInfo.FullName}");
        }
    }
}