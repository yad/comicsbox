using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using comicsbox.Models;

namespace comicsbox.Controllers;

public class HomeController : Controller
{
    private readonly List<BookCategory> _categories;

    public HomeController(IOptions<List<BookCategory>> categories)
    {
        _categories = categories.Value;
    }

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

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel
        {
            RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier
        });
    }

    /* ============================
     * Placeholder (même rendu que ComicsController)
     * ============================ */
    private string GetPlaceholder()
    {
        return "data:image/svg+xml,%3Csvg xmlns='http://www.w3.org/2000/svg' width='200' height='300' viewBox='0 0 200 300'%3E%3Crect width='200' height='300' fill='%23e0e0e0'/%3E%3Ctext x='100' y='150' font-size='20' text-anchor='middle' fill='%23666'%3E📖%3C/text%3E%3C/svg%3E";
    }
}
