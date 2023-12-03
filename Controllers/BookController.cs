using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;

namespace Comicsbox.Controllers;

[ApiController]
[Route("api/[controller]")]
public class BookController : ControllerBase
{
    private readonly BookInfoService _bookInfoService;

    public BookController(BookInfoService bookInfoService)
    {
        _bookInfoService = bookInfoService;
    }

    [HttpGet("{category}")]
    public Task<BookContainer<Book>> Get(string category)
    {
        return _bookInfoService.GetBookListAsync(category);
    }

    [HttpGet("{category}/{serie}")]
    public Task<BookContainer<Book>> Get(string category, string serie)
    {
        return _bookInfoService.GetBookListAsync(category, serie);
    }
}

