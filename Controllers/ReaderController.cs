using Microsoft.AspNetCore.Mvc;

namespace Comicsbox.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ReaderController : ControllerBase
{
    private readonly BookInfoService _bookInfoService;
    private readonly PdfReaderService _pdfReaderService;

    public ReaderController(BookInfoService bookInfoService, PdfReaderService pdfReaderService)
    {
        _bookInfoService = bookInfoService;
        _pdfReaderService = pdfReaderService;
    }

    [HttpPost("{category}/{serie}/{book}/{page}")]
    public IActionResult Post(string category, string serie, string book, string page)
    {
        var pdfPath = _bookInfoService.GetSerieBookPath(category, serie, book);
        var seriePath = Path.Combine("wwwroot", "temp", category, serie, book);
        if (!System.IO.Directory.Exists(seriePath))
        {
            Task.Run(() => {
                var tempPath = Path.Combine("wwwroot", "temp", Guid.NewGuid().ToString());
                Directory.CreateDirectory(tempPath);

                _pdfReaderService.LoadFile(pdfPath, isReversed: category.ToLower() == "mangas").Extract(tempPath);

                if (System.IO.Directory.Exists(seriePath))
                {
                    System.IO.Directory.Delete(seriePath, true);
                }

                Directory.CreateDirectory(seriePath);
                Directory.Delete(seriePath);
                System.IO.Directory.Move(tempPath, seriePath);
            });
        }

        return Ok();
    }

    [HttpGet("{category}/{serie}/{book}/{page}")]
    public IActionResult Get(string category, string serie, string book, string page)
    {
        var jpgPath = Path.Combine("wwwroot", "temp", category, serie, book, $"{page}.jpg");
        if (!System.IO.File.Exists(jpgPath))
        {
            var eofPath = jpgPath.Replace(".jpg", ".eof");
            if (System.IO.File.Exists(eofPath))
            {
                return NotFound();
            }

            return NoContent();
        }

        return Content($"/temp/{category}/{serie}/{book}/{page}.jpg");
    }

    [HttpGet("{category}/{serie}/{book}")]
    public IActionResult Get(string category, string serie, string book)
    {
        var path = _bookInfoService.GetSeriePath(category, serie);
        var pdfPath = Path.Combine(path, $"{book}.pdf");
        return File(System.IO.File.OpenRead(pdfPath), "application/pdf", $"{serie}_{Path.GetFileName(pdfPath)}");
    }
}

