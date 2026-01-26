using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using comicsbox.Models;
using Microsoft.Extensions.Options;

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
        return View(_categories.OrderBy(c => c.Name).ToList());
    }

    public IActionResult Privacy()
    {
        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
