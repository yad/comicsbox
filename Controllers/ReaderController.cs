﻿using System.Threading.Channels;
using Microsoft.AspNetCore.Mvc;

namespace Comicsbox.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ReaderController : ControllerBase
{
    private readonly Channel<Reader> _channel;

    private readonly BookInfoService _bookInfoService;

    private readonly PdfReaderService _pdfReaderService;

    public ReaderController(Channel<Reader> channel, BookInfoService bookInfoService, PdfReaderService pdfReaderService)
    {
        _channel = channel;
        _bookInfoService = bookInfoService;
        _pdfReaderService = pdfReaderService;
    }

    [HttpPost("{category}/{serie}/{book}/{page}")]
    public IActionResult Post(string category, string serie, string book, int page)
    {
        var pdfPath = _bookInfoService.GetSerieBookPath(category, serie, book);
        var seriePath = Path.Combine("wwwroot", "temp", category, serie, book);
        var file = Path.Combine(seriePath, $"{page}.jpg");
        if (!System.IO.File.Exists(file))
        {
            var command = new Reader(seriePath, () => Task.Run(() => {
                if (!Directory.Exists(seriePath))
                {
                    Directory.CreateDirectory(seriePath);
                }

                _pdfReaderService.LoadFile(pdfPath, isReversed: category.ToLower() == "mangas").Extract(seriePath, page);
            }));

            if (!_channel.Writer.TryWrite(command))
            {
                return Conflict();
            }
        }

        return Ok();
    }

    [HttpGet("{category}/{serie}/{book}/{page}")]
    public IActionResult Get(string category, string serie, string book, int page)
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

        return Ok();
    }
}
