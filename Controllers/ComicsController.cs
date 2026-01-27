using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using comicsbox.Models;
using comicsbox.Services;
using System.IO;

namespace comicsbox.Controllers;

public class ComicsController : Controller
{
    private readonly List<BookCategory> _categories;
    private readonly ZipWorker _zipWorker;

    public ComicsController(
        ZipWorker zipWorker,
        IOptions<List<BookCategory>> categories)
    {
        _categories = categories.Value;
        _zipWorker = zipWorker;
    }

    /* ============================
     * INDEX – Catégories
     * ============================ */
    public IActionResult Index()
    {
        var vm = new LibraryViewModel
        {
            Title = "ComicsBox",
            Items = _categories
                .OrderBy(c => c.Name)
                .Select(c => new CardItemViewModel
                {
                    Title = c.Name,
                    ImageUrl = GetPlaceholder(),
                    Action = "Series",
                    Controller = "Comics",
                    RouteValues = new { category = c.Name }
                })
                .ToList()
        };

        ViewData["ShowBackButton"] = false;
        return View(vm);
    }

    /* ============================
     * SERIES
     * ============================ */
    public IActionResult Series(string category)
    {
        var cat = _categories.FirstOrDefault(c => c.Name == category);
        if (cat == null || !Directory.Exists(cat.Path))
            return NotFound();

        var vm = new LibraryViewModel
        {
            Title = $"{category} – Séries",
            Category = category,
            Items = Directory.GetDirectories(cat.Path)
                .Select(Path.GetFileName)
                .Where(name => !string.IsNullOrWhiteSpace(name))
                .OrderBy(name => name)
                .Select(series => new CardItemViewModel
                {
                    Title = series!,
                    ImageUrl = GetCoverPath(category, series!),
                    Action = "Books",
                    Controller = "Comics",
                    RouteValues = new { category, series }
                })
                .ToList()
        };

        ViewData["ShowBackButton"] = true;
        ViewData["BackUrl"] = Url.Action("Index", "Home");
        return View(vm);
    }

    /* ============================
     * BOOKS
     * ============================ */
    public IActionResult Books(string category, string series)
    {
        var cat = _categories.FirstOrDefault(c => c.Name == category);
        if (cat == null)
            return NotFound();

        var seriesPath = Path.Combine(cat.Path, series);
        if (!Directory.Exists(seriesPath))
            return NotFound();

        var books = Directory.GetFiles(seriesPath, "*.pdf")
            .Select(file => new CardItemViewModel
            {
                Title = Path.GetFileNameWithoutExtension(file),
                ImageUrl = GetCoverPath(category, series, Path.GetFileName(file)),
                Action = "Download",
                Controller = "Comics",
                RouteValues = new { category, series, file = Path.GetFileName(file) }
            })
            .ToList();

        var vm = new LibraryViewModel
        {
            Title = series,
            Category = category,
            Series = series,
            Items = books
        };

        // // Ajouter la carte ZIP si plusieurs ebooks
        // if (books.Count > 1)
        // {
        //     vm.Items.Add(new CardItemViewModel
        //     {
        //         Title = "Télécharger la série complète",
        //         ImageUrl = "📦",
        //         Action = "DownloadSeries",
        //         Controller = "Comics",
        //         RouteValues = new { category, series },
        //         IsEmoji = true
        //     });
        // }

        ViewData["ShowBackButton"] = true;
        ViewData["BackUrl"] = Url.Action("Series", "Comics", new { category });
        return View(vm);
    }

    /* ============================
    * SEARCH
    * ============================ */
    public IActionResult Search(string query)
    {
        var items = new List<CardItemViewModel>();

        // Si la recherche est vide, on reste sur la page et n'affiche rien
        if (!string.IsNullOrWhiteSpace(query))
        {
            foreach (var category in _categories)
            {
                if (!Directory.Exists(category.Path))
                    continue;

                var seriesMatches = Directory.GetDirectories(category.Path)
                    .Select(Path.GetFileName)
                    .Where(name =>
                        !string.IsNullOrWhiteSpace(name) &&
                        name.Contains(query, StringComparison.OrdinalIgnoreCase));

                foreach (var series in seriesMatches)
                {
                    items.Add(new CardItemViewModel
                    {
                        Title = series!,
                        ImageUrl = GetCoverPath(category.Name, series!),
                        Action = "Books",
                        Controller = "Comics",
                        RouteValues = new
                        {
                            category = category.Name,
                            series
                        }
                    });
                }
            }
        }

        var vm = new LibraryViewModel
        {
            Title = query ?? "",       // si vide, on met ""
            Items = items.OrderBy(i => i.Title).ToList()
        };

        ViewData["ShowBackButton"] = true;
        ViewData["BackUrl"] = Url.Action("Index", "Home");

        return View(vm);
    }

    /* ============================
     * DOWNLOAD PDF
     * ============================ */
    public IActionResult Download(string category, string series, string file)
    {
        if (string.IsNullOrWhiteSpace(category) ||
            string.IsNullOrWhiteSpace(series) ||
            string.IsNullOrWhiteSpace(file))
            return BadRequest();

        var cat = _categories.FirstOrDefault(c => c.Name == category);
        if (cat == null)
            return NotFound();

        var filePath = Path.Combine(cat.Path, series, Path.GetFileName(file));
        if (!System.IO.File.Exists(filePath))
            return NotFound();

        return PhysicalFile(filePath, "application/pdf", $"{series} - {Path.GetFileName(file)}");
    }

    /* ============================
     * ZIP – Génération async
     * ============================ */
    [HttpPost]
    public IActionResult DownloadSeries(string category, string series)
    {
        var zipDir = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "cache", "zip");
        Directory.CreateDirectory(zipDir);

        var zipPath = Path.Combine(zipDir, $"{category}-{series}.zip");
        var url = Url.Action("DownloadSeriesFile", "Comics", new { category, series });

        if (!System.IO.File.Exists(zipPath))
        {
            _zipWorker.Enqueue(category, series);
            return Json(new { status = "processing", url });
        }

        return Json(new { status = "ready", url });
    }

    [HttpGet]
    public IActionResult DownloadSeriesFile(string category, string series)
    {
        var zipPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "cache", "zip", $"{category}-{series}.zip");

        if (!System.IO.File.Exists(zipPath))
            return NotFound();

        return PhysicalFile(zipPath, "application/zip", $"{series}.zip");
    }

    [HttpGet]
    public IActionResult CheckZipStatus(string category, string series)
    {
        var zipPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "cache", "zip", $"{category}-{series}.zip");
        var url = Url.Action("DownloadSeriesFile", "Comics", new { category, series });

        return Json(System.IO.File.Exists(zipPath)
            ? new { status = "ready", url }
            : new { status = "processing", url });
    }

    /* ============================
     * THUMBNAILS
     * ============================ */
    private string GetCoverPath(string category, string series, string? bookFileName = null)
    {
        var cacheDir = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "cache", "thumbnails");
        var cat = _categories.FirstOrDefault(c => c.Name == category);
        if (cat == null || !Directory.Exists(cat.Path)) return GetPlaceholder();

        var seriesPath = Path.Combine(cat.Path, series);
        if (!Directory.Exists(seriesPath)) return GetPlaceholder();

        var pdfs = Directory.GetFiles(seriesPath, "*.pdf").OrderBy(f => f).ToList();
        if (!pdfs.Any()) return GetPlaceholder();

        var pdf = bookFileName != null ? pdfs.FirstOrDefault(f => Path.GetFileName(f) == bookFileName) : pdfs.First();
        if (pdf == null) return GetPlaceholder();

        var thumbName = ThumbnailHelper.GetThumbnailFileName(pdf);
        var cachePath = Path.Combine(cacheDir, thumbName);

        return System.IO.File.Exists(cachePath)
            ? $"/cache/thumbnails/{thumbName}"
            : GetPlaceholder();
    }

    private string GetPlaceholder()
    {
        return "data:image/svg+xml,%3Csvg xmlns='http://www.w3.org/2000/svg' width='200' height='300' viewBox='0 0 200 300'%3E%3Crect width='200' height='300' fill='%23e0e0e0'/%3E%3Ctext x='100' y='150' font-size='20' text-anchor='middle' fill='%23666'%3E📖%3C/text%3E%3C/svg%3E";
    }
}
