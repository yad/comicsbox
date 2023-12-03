using System.IO.Compression;
using System.Threading.Channels;
using Microsoft.AspNetCore.Mvc;

namespace Comicsbox.Controllers;

[ApiController]
[Route("api/[controller]")]
public class DownloadController : ControllerBase
{
    private readonly Channel<Func<Task>> _channel;

    private readonly BookInfoService _bookInfoService;

    public DownloadController(Channel<Func<Task>> channel, BookInfoService bookInfoService)
    {
        _channel = channel;
        _bookInfoService = bookInfoService;
    }

    [HttpPost("{category}/{serie}")]
    public IActionResult Post(string category, string serie)
    {
        var path = _bookInfoService.GetSeriePath(category, serie);
        var zipPath = Path.Combine("wwwroot", "temp", $"{serie}.zip");
        if (!System.IO.File.Exists(zipPath))
        {
            Func<Task> command = () => Task.Run(() => {
                var tempPath = Path.Combine("wwwroot", "temp", $"{Guid.NewGuid()}.zip");

                ZipFile.CreateFromDirectory(path, tempPath, CompressionLevel.NoCompression, false);

                if (System.IO.File.Exists(zipPath))
                {
                    System.IO.File.Delete(zipPath);
                }

                System.IO.File.Move(tempPath, zipPath);
            });

            if (!_channel.Writer.TryWrite(command))
            {
                return Conflict();
            }
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

        return Ok();
    }

    [HttpGet("{category}/{serie}/{book}")]
    public IActionResult Get(string category, string serie, string book)
    {
        var path = _bookInfoService.GetSeriePath(category, serie);
        var pdfPath = Path.Combine(path, $"{book}.pdf");
        return File(System.IO.File.OpenRead(pdfPath), "application/pdf", $"{serie}_{Path.GetFileName(pdfPath)}");
    }
}
