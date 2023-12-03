using Microsoft.AspNetCore.Mvc;

namespace Comicsbox.Controllers;

[ApiController]
[Route("api/[controller]")]
public class SearchController : ControllerBase
{
    private readonly BookInfoService _bookInfoService;

    public SearchController(BookInfoService bookInfoService)
    {
        _bookInfoService = bookInfoService;
    }

    [HttpGet()]
    public Task<BookContainer<Book>> Get()
    {
        return _bookInfoService.GetBookListAsync();
    }
}

