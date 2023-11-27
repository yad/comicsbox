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

    [HttpPost("{category}/{serie}")]
    public IActionResult Post(string category, string serie)
    {
        var path = _bookInfoService.GetSeriePath(category, serie);
        var zipPath = Path.Combine("wwwroot", "temp", $"{serie}.zip");
        if (!System.IO.File.Exists(zipPath))
        {
            Task.Run(() => {
                var tempPath = Path.Combine("wwwroot", "temp", $"{Guid.NewGuid()}.zip");

                ZipFile.CreateFromDirectory(path, tempPath, CompressionLevel.NoCompression, false);

                if (System.IO.File.Exists(zipPath))
                {
                    System.IO.File.Delete(zipPath);
                }

                System.IO.File.Move(tempPath, zipPath);
            });
        }

        return Ok();
    }

    [HttpGet("{category}/{serie}")]
    public IActionResult Get(string category, string serie)
    {
        var zipPath = Path.Combine("wwwroot", "temp", $"{serie}.zip");
        if (!System.IO.File.Exists(zipPath))
        {
            return NoContent();
        }

        return Content($"/temp/{serie}.zip");
    }

    [HttpGet("{category}/{serie}/{book}")]
    public IActionResult Get(string category, string serie, string book)
    {
        var path = _bookInfoService.GetSeriePath(category, serie);
        var pdfPath = Path.Combine(path, $"{book}.pdf");
        return File(System.IO.File.OpenRead(pdfPath), "application/pdf", $"{serie}_{Path.GetFileName(pdfPath)}");
    }
}
