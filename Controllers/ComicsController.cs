using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using comicsbox.Models;
using comicsbox.Services;
using System.IO;
using Microsoft.Extensions.Caching.Memory;

namespace comicsbox.Controllers;

public class ComicsController : Controller
{
    private readonly IMemoryCache _cache;
    private readonly List<BookCategory> _categories;
    private readonly ZipWorker _zipWorker;

    public ComicsController(
        IMemoryCache cache,
        ZipWorker zipWorker,
        IOptions<List<BookCategory>> categories)
    {
        _cache = cache;
        _categories = categories.Value;
        _zipWorker = zipWorker;
    }

    /* ============================
     * INDEX – Catégories (/)
     * ============================ */
    [HttpGet("")]
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
                    ImageUrl = GetCategoryCover(c.Name),
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
     * SERIES (/Mangas)
     * ============================ */
    [HttpGet("{category}")]
    public IActionResult Series(string category, string? sort)
    {
        var sortMode = string.IsNullOrWhiteSpace(sort) ? "new" : sort;
        var cacheKey = $"series::{category}::{sortMode}";

        if (_cache.TryGetValue(cacheKey, out LibraryViewModel? vm))
        {
            SetBackToIndex();
            return View(vm);
        }

        var cat = _categories.FirstOrDefault(c => c.Name == category);
        if (cat == null || !Directory.Exists(cat.Path))
            return NotFound();

        var directories = Directory.GetDirectories(cat.Path);

        var sortedDirectories = sortMode switch
        {
            "new" => directories
                .OrderByDescending(d => Directory.GetLastWriteTime(d)),

            "count" => directories
                .OrderByDescending(d => Directory.GetFiles(d).Length),

            _ => directories
                .OrderBy(d => Path.GetFileName(d))
        };

        vm = new LibraryViewModel
        {
            Title = $"{category} – Séries",
            Category = category,

            Items = sortedDirectories
                .Select(dir => new
                {
                    FullPath = dir,
                    Name = Path.GetFileName(dir)
                })
                .Where(x => !string.IsNullOrWhiteSpace(x.Name))
                .Select(x => new CardItemViewModel
                {
                    Title = x.Name!,
                    Count = Directory.GetFiles(x.FullPath, "*.pdf").Length,
                    Date = Directory.GetFiles(x.FullPath, "*.pdf").Select(f => new FileInfo(f).LastWriteTime).OrderByDescending(d => d).FirstOrDefault().ToString("yyyy-MM-dd"),
                    ImageUrl = GetCoverPath(category, x.Name!),
                    Action = "Books",
                    Controller = "Comics",
                    RouteValues = new { category, series = x.Name }
                })
                .ToList()
        };

        SetBackToIndex();

        _cache.Set(cacheKey, vm, TimeSpan.FromHours(4));
        return View(vm);
    }

    /* ============================
     * BOOKS (/Mangas/OnePiece)
     * ============================ */
    [HttpGet("{category}/{series}")]
    public IActionResult Books(string category, string series)
    {
        var cacheKey = $"books::{category}::{series}";

        if (_cache.TryGetValue(cacheKey, out LibraryViewModel? vm))
        {
            SetBackToSeries();
            return View(vm);
        }

        var cat = _categories.FirstOrDefault(c => c.Name == category);
        if (cat == null)
            return NotFound();

        var seriesPath = Path.Combine(cat.Path, series);
        if (!Directory.Exists(seriesPath))
            return NotFound();

        var books = Directory.GetFiles(seriesPath, "*.pdf")
            .OrderBy(name => name)
            .Select(file => new CardItemViewModel
            {
                Title = Path.GetFileNameWithoutExtension(file),
                Date = new FileInfo(file).LastWriteTime.ToString("yyyy-MM-dd"),
                ImageUrl = GetCoverPath(category, series, Path.GetFileName(file)),
                Action = "Download",
                Controller = "Comics",
                RouteValues = new { category, series, file = Path.GetFileName(file) },
                Category = category,
                Series = series
            })
            .ToList();

        vm = new LibraryViewModel
        {
            Title = series,
            Category = category,
            Series = series,
            Items = books
        };

        if (books.Count > 1)
        {
            vm.Items.Add(new CardItemViewModel
            {
                Title = "Télécharger la série complète",
                ImageUrl = "📦",
                Action = "DownloadSeries",
                Controller = "Comics",
                RouteValues = new { category, series },
                IsEmoji = true,
                Category = category,
                Series = series
            });
        }

        SetBackToSeries();

        _cache.Set(cacheKey, vm, TimeSpan.FromHours(4));
        return View(vm);
    }

    /* ============================
     * SEARCH (/search?query=xxx)
     * ============================ */
    [HttpGet("search")]
    public IActionResult Search(string query)
    {
        var items = new List<CardItemViewModel>();

        if (!string.IsNullOrWhiteSpace(query))
        {
            foreach (var category in _categories)
            {
                if (!Directory.Exists(category.Path))
                    continue;

                foreach (var series in Directory.GetDirectories(category.Path)
                             .Select(Path.GetFileName)
                             .Where(n => n != null && n.Contains(query, StringComparison.OrdinalIgnoreCase)))
                {
                    items.Add(new CardItemViewModel
                    {
                        Title = series!,
                        ImageUrl = GetCoverPath(category.Name, series!),
                        Action = "Books",
                        Controller = "Comics",
                        RouteValues = new { category = category.Name, series },
                        Category = category.Name,
                        Series = series!
                    });
                }
            }
        }

        SetBackToIndex();

        return View(new LibraryViewModel
        {
            Title = query ?? "",
            Items = items.OrderBy(i => i.Title).ToList()
        });
    }

    /* ============================
    * DOWNLOAD PDF
    * URL RESTful: /Comics/Download/{category}/{series}/{file}
    * Exemple: /Comics/Download/BD/Alix origines/001.pdf
    * ============================ */
    [HttpGet("Comics/Download/{category}/{series}/{file}")]
    public IActionResult Download(string category, string series, string file)
    {
        var cat = _categories.FirstOrDefault(c => c.Name == category);
        if (cat == null)
            return NotFound();

        var filePath = Path.Combine(cat.Path, series, Path.GetFileName(file));
        if (!System.IO.File.Exists(filePath))
            return NotFound();

        // Téléchargement PDF avec nom correct
        return PhysicalFile(filePath, "application/pdf", $"{series} - {file}");
    }

    /* ============================
    * ZIP – génération async
    * URL RESTful: /Comics/Zip/{category}/{series}
    * ============================ */
    [HttpGet("Comics/Zip/{category}/{series}")]
    public IActionResult DownloadSeries(string category, string series)
    {
        var zipDir = Path.Combine("wwwroot", "cache", "zip");
        Directory.CreateDirectory(zipDir);

        var zipPath = Path.Combine(zipDir, $"{category}-{series}.zip");
        var url = Url.Action("DownloadSeriesFile", new { category, series });

        if (!System.IO.File.Exists(zipPath))
        {
            _zipWorker.Enqueue(category, series);
            return Json(new { status = "processing", url });
        }

        return Json(new { status = "ready", url });
    }

    /* ============================
    * ZIP – téléchargement final
    * URL RESTful: /Comics/ZipFile/{category}/{series}
    * ============================ */
    [HttpGet("Comics/ZipFile/{category}/{series}")]
    public IActionResult DownloadSeriesFile(string category, string series)
    {
        var zipPath = Path.Combine(
            Directory.GetCurrentDirectory(),
            "wwwroot",
            "cache",
            "zip",
            $"{category}-{series}.zip"
        );

        if (!System.IO.File.Exists(zipPath))
            return NotFound();

        return PhysicalFile(
            zipPath,
            "application/zip",
            $"{series}.zip"
        );
    }

    /* ============================
    * Vérifier statut ZIP
    * URL RESTful: /Comics/ZipStatus/{category}/{series}
    * ============================ */
    [HttpGet("Comics/ZipStatus/{category}/{series}")]
    public IActionResult CheckZipStatus(string category, string series)
    {
        var zipPath = Path.Combine("wwwroot", "cache", "zip", $"{category}-{series}.zip");
        var url = Url.Action("DownloadSeriesFile", new { category, series });

        return Json(System.IO.File.Exists(zipPath)
            ? new { status = "ready", url }
            : new { status = "processing", url });
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View();
    }


    /* ============================
     * Helpers navigation
     * ============================ */
    private void SetBackToIndex()
    {
        ViewData["ShowBackButton"] = true;
    }

    private void SetBackToSeries()
    {
        ViewData["ShowBackButton"] = true;
    }

    private string GetCategoryCover(string category)
    {
        var cat = _categories.FirstOrDefault(c => c.Name == category);
        if (cat == null || !Directory.Exists(cat.Path))
            return GetPlaceholder();

        // Récupère toutes les séries valides
        var seriesList = Directory.GetDirectories(cat.Path)
            .Select(Path.GetFileName)
            .Where(name => !string.IsNullOrWhiteSpace(name))
            .ToList();

        if (!seriesList.Any())
            return GetPlaceholder();

        // Choisir une série au hasard
        var random = new Random();
        var series = seriesList[random.Next(seriesList.Count)]!;

        var seriesPath = Path.Combine(cat.Path, series);
        var pdfs = Directory.GetFiles(seriesPath, "*.pdf")
            .OrderBy(f => f)
            .ToList();

        if (!pdfs.Any())
            return GetPlaceholder();

        // Choisir un PDF aléatoire parmi la série sélectionnée
        var pdf = pdfs[random.Next(pdfs.Count)];

        var cacheDir = Path.Combine(
            Directory.GetCurrentDirectory(),
            "wwwroot",
            "cache",
            "thumbnails"
        );

        var thumbName = ThumbnailHelper.GetThumbnailFileName(pdf);
        var cachePath = Path.Combine(cacheDir, thumbName);

        return System.IO.File.Exists(cachePath)
            ? $"~/cache/thumbnails/{thumbName}"
            : GetPlaceholder();
    }

    private string GetPlaceholder()
    {
        return
            "data:image/svg+xml;utf8," +
            "<svg xmlns='http://www.w3.org/2000/svg' width='200' height='300' viewBox='0 0 200 300'>" +

            // Fond
            "<defs>" +
            "<linearGradient id='bg' x1='0' y1='0' x2='1' y2='1'>" +
            "<stop offset='0%' stop-color='%23f5f5f5'/>" +
            "<stop offset='100%' stop-color='%23e0e0e0'/>" +
            "</linearGradient>" +
            "</defs>" +

            "<rect width='200' height='300' fill='url(%23bg)'/>" +

            // Cadre comics
            "<rect x='10' y='10' width='180' height='280' rx='18' ry='18' " +
            "fill='%23ffffff' stroke='%23ff6b35' stroke-width='4'/>" +

            // Trame comics légère
            "<g opacity='0.08'>" +
            "<circle cx='40' cy='60' r='6' fill='%23000'/>" +
            "<circle cx='70' cy='80' r='6' fill='%23000'/>" +
            "<circle cx='100' cy='60' r='6' fill='%23000'/>" +
            "<circle cx='130' cy='80' r='6' fill='%23000'/>" +
            "<circle cx='160' cy='60' r='6' fill='%23000'/>" +
            "</g>" +

            // Icône outil / génération
            "<text x='100' y='120' font-size='48' text-anchor='middle'>⚙️</text>" +

            // Ligne
            "<line x1='40' y1='150' x2='160' y2='150' stroke='%23ff6b35' stroke-width='3'/>" +

            // Texte principal
            "<text x='100' y='180' font-size='14' text-anchor='middle' " +
            "font-family='Segoe UI, Arial, sans-serif' fill='%23333' font-weight='700'>" +
            "Couverture en cours" +
            "</text>" +

            // Texte secondaire
            "<text x='100' y='200' font-size='12' text-anchor='middle' " +
            "font-family='Segoe UI, Arial, sans-serif' fill='%23666'>" +
            "Génération en arrière-plan…" +
            "</text>" +

            "</svg>";
    }

    private string GetCoverPath(string category, string series, string? bookFileName = null)
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

        var pdf = bookFileName != null
            ? pdfs.FirstOrDefault(f => Path.GetFileName(f) == bookFileName)
            : pdfs.First();

        if (pdf == null)
            return GetPlaceholder();

        var thumbName = ThumbnailHelper.GetThumbnailFileName(pdf);
        var cachePath = Path.Combine(cacheDir, thumbName);

        return System.IO.File.Exists(cachePath)
            ? $"~/cache/thumbnails/{thumbName}"
            : GetPlaceholder();
    }
}
