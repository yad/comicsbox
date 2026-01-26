using Microsoft.AspNetCore.Mvc;
using comicsbox.Models;
using Microsoft.Extensions.Options;
using comicsbox.Services;
using System.IO.Compression;

namespace comicsbox.Controllers;

public class ComicsController : Controller
{
    private readonly List<BookCategory> _categories;
    private readonly ZipWorker _zipWorker;

    public ComicsController(ZipWorker zipWorker, IOptions<List<BookCategory>> categories)
    {
        _categories = categories.Value;
        _zipWorker = zipWorker;
    }

    public IActionResult Index()
    {
        return View(_categories.OrderBy(c => c.Name).ToList());
    }

    public IActionResult Series(string category)
    {
        var cat = _categories.FirstOrDefault(c => c.Name == category);
        if (cat == null || !Directory.Exists(cat.Path))
        {
            return NotFound();
        }

        var series = Directory.GetDirectories(cat.Path)
            .Select(Path.GetFileName)
            .Where(name => !string.IsNullOrEmpty(name))
            .ToList();

        var seriesWithCovers = series.Select(s => new SeriesViewModel
        {
            Name = s,
            CoverUrl = GetCoverPath(category, s)
        }).ToList();

        ViewBag.Category = category;
        return View(seriesWithCovers);
    }

    public IActionResult Books(string category, string series)
    {
        var cat = _categories.FirstOrDefault(c => c.Name == category);
        if (cat == null)
        {
            return NotFound();
        }

        var seriesPath = Path.Combine(cat.Path, series);
        if (!Directory.Exists(seriesPath))
        {
            return NotFound();
        }

        var books = Directory.GetFiles(seriesPath, "*.pdf")
            .Select(file => new MangaItem
            {
                Series = series,
                Title = Path.GetFileNameWithoutExtension(file),
                FileName = Path.GetFileName(file),
                FullPath = file,
                CoverUrl = GetCoverPath(category, series, Path.GetFileName(file))
            })
            .ToList();

        ViewBag.Category = category;
        ViewBag.Series = series;
        return View(books);
    }

    public IActionResult Search(string query)
    {
        if (string.IsNullOrWhiteSpace(query))
        {
            return Redirect("/");
        }

        var results = new List<SearchResult>();
        foreach (var category in _categories)
        {
            if (Directory.Exists(category.Path))
            {
                var series = Directory.GetDirectories(category.Path)
                    .Select(Path.GetFileName)
                    .Where(name => !string.IsNullOrEmpty(name) && name.Contains(query, StringComparison.OrdinalIgnoreCase))
                    .ToList();

                foreach (var seriesName in series)
                {
                    results.Add(new SearchResult { Category = category.Name, Series = seriesName });
                }
            }
        }

        ViewBag.Query = query;
        return View(results);
    }

    private string GetCoverPath(string category, string series, string ? bookFileName = null)
    {
        var cacheDir = Path.Combine(
            Directory.GetCurrentDirectory(),
            "wwwroot",
            "cache",
            "thumbnails"
        );

        var cat = _categories.FirstOrDefault(c => c.Name == category);
        if (cat == null || !Directory.Exists(cat.Path))
            return GetPlaceholder();

        var seriesPath = Path.Combine(cat.Path, series);
        if (!Directory.Exists(seriesPath))
            return GetPlaceholder();

        var pdfs = Directory.GetFiles(seriesPath, "*.pdf")
                            .OrderBy(f => f)
                            .ToList();

        if (!pdfs.Any())
            return GetPlaceholder();

        var pdf = bookFileName != null ? pdfs.FirstOrDefault(f => Path.GetFileName(f) == bookFileName) : pdfs.First();

        var fileName = ThumbnailHelper.GetThumbnailFileName(pdf);
        var cachePath = Path.Combine(cacheDir, fileName);

        if (System.IO.File.Exists(cachePath))
        {
            return $"/cache/thumbnails/{fileName}";
        }

        return GetPlaceholder();
    }


    private string GetPlaceholder()
    {
        return "data:image/svg+xml,%3Csvg xmlns='http://www.w3.org/2000/svg' width='200' height='300' viewBox='0 0 200 300'%3E%3Crect width='200' height='300' fill='%23e0e0e0'/%3E%3Ctext x='100' y='150' font-size='20' text-anchor='middle' fill='%23666'%3E📖%3C/text%3E%3C/svg%3E";
    }

    private string GetSha256Hash(string input)
    {
        using (var sha256 = System.Security.Cryptography.SHA256.Create())
        {
            var hashedBytes = sha256.ComputeHash(System.Text.Encoding.UTF8.GetBytes(input));
            return System.Convert.ToHexString(hashedBytes).ToLowerInvariant();
        }
    }

    public IActionResult Download(string category, string series, string file)
    {
        if (string.IsNullOrWhiteSpace(category) || string.IsNullOrWhiteSpace(series) || string.IsNullOrWhiteSpace(file))
            return BadRequest();

        var cat = _categories.FirstOrDefault(c => c.Name == category);
        if (cat == null || !Directory.Exists(cat.Path))
            return NotFound();

        var seriesPath = Path.Combine(cat.Path, series);
        if (!Directory.Exists(seriesPath))
            return NotFound();

        // Sécurise le nom du fichier pour éviter path traversal
        var safeFileName = Path.GetFileName(file);
        var filePath = Path.Combine(seriesPath, safeFileName);

        if (!System.IO.File.Exists(filePath))
            return NotFound();

        var fileBytes = System.IO.File.ReadAllBytes(filePath);
        var contentType = "application/pdf";

        // Nouveau nom : "Series - File.pdf"
        var downloadName = $"{series} - {safeFileName}";

        return File(fileBytes, contentType, downloadName);
    }

    public IActionResult DownloadSeriesAsync(string category, string series)
    {
        var zipDir = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "cache", "zip");
        var zipPath = Path.Combine(zipDir, $"{category}-{series}.zip");

        if (System.IO.File.Exists(zipPath))
        {
            // ZIP déjà prêt : on renvoie directement
            return PhysicalFile(zipPath, "application/zip", $"{series}.zip");
        }

        // Sinon on met dans la queue
        _zipWorker.Enqueue(category, series);

        // Retourne un status "en cours" et spinner côté client
        return Json(new { status = "processing" });
    }

    // Une API pour vérifier si le ZIP est prêt
    public IActionResult CheckZipStatus(string category, string series)
    {
        var zipPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "cache", "zip", $"{category}-{series}.zip");
        if (System.IO.File.Exists(zipPath))
        {
            return Json(new { status = "ready", url = $"/cache/zip/{category}-{series}.zip" });
        }
        return Json(new { status = "processing" });
    }

    /// <summary>
    /// Télécharge le ZIP d'une série déjà généré
    /// </summary>
    [HttpGet]
    public IActionResult DownloadSeriesFile(string category, string series)
    {
        if (string.IsNullOrWhiteSpace(category) || string.IsNullOrWhiteSpace(series))
            return BadRequest();

        var zipDir = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "cache", "zip");
        var zipPath = Path.Combine(zipDir, $"{category}-{series}.zip");

        if (!System.IO.File.Exists(zipPath))
            return NotFound();

        var downloadFileName = $"{series}.zip";

        // Sert le fichier directement sans passer par Static Assets
        return PhysicalFile(zipPath, "application/zip", downloadFileName);
    }
}