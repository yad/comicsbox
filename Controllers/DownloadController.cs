using System.IO.Compression;
using Microsoft.AspNetCore.Mvc;

namespace Comicsbox.Controllers;

[ApiController]
[Route("api/[controller]")]
public class DownloadController : ControllerBase
{
    private readonly BookInfoService _bookInfoService;

    public DownloadController(BookInfoService bookInfoService)
    {
        _bookInfoService = bookInfoService;
    }

    [HttpGet("{category}/{serie}")]
    public IActionResult Get(string category, string serie)
    {
        var path = _bookInfoService.GetSeriePath(category, serie);
        var zipPath = Path.Combine("wwwroot", "temp", $"{serie}.zip");
        if (!System.IO.File.Exists(zipPath))
        {
            var tempPath = Path.Combine("wwwroot", "temp", $"{Guid.NewGuid()}.zip");

            ZipFile.CreateFromDirectory(path, tempPath);

            if (System.IO.File.Exists(zipPath))
            {
                System.IO.File.Delete(zipPath);
            }

            System.IO.File.Move(tempPath, zipPath);
        }

        return File(System.IO.File.OpenRead(zipPath), "application/zip", Path.GetFileName(zipPath));
    }

    [HttpGet("{category}/{serie}/{book}")]
    public IActionResult Get(string category, string serie, string book)
    {
        var path = _bookInfoService.GetSeriePath(category, serie);
        var pdfPath = Path.Combine(path, $"{book}.pdf");
        return File(System.IO.File.OpenRead(pdfPath), "application/pdf", $"{serie}_{Path.GetFileName(pdfPath)}");
    }

    // [HttpGet("{category}/{book}/{chapter}/{page}")]
    // public PageDetail Get(string category, string book, string chapter, int page)
    // {
    //     return _bookInfoService.GetDetail(category, book, chapter, page);
    // }
}

