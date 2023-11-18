using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;

namespace Comicsbox.Controllers;

[ApiController]
[Route("api/[controller]")]
public class BookController : ControllerBase
{
    private readonly IMemoryCache _memoryCache;
    private readonly BookInfoService _bookInfoService;

    public BookController(IMemoryCache memoryCache, BookInfoService bookInfoService)
    {
        _bookInfoService = bookInfoService;
        _memoryCache = memoryCache;
    }

    [HttpGet("{category}")]
    public BookContainer<Book> Get(string category)
    {
        var cacheKey = $"{category}";
        if (!_memoryCache.TryGetValue(cacheKey, out BookContainer<Book> cacheValue))
        {
            cacheValue = _bookInfoService.GetBookList(category, "");

            var cacheEntryOptions = new MemoryCacheEntryOptions()
                .SetSlidingExpiration(TimeSpan.FromHours(1));

            _memoryCache.Set(cacheKey, cacheValue, cacheEntryOptions);
        }

        return cacheValue;
    }

    [HttpGet("{category}/{serie}")]
    public BookContainer<Book> Get(string category, string serie)
    {
        var cacheKey = $"{category}-{serie}";
        if (!_memoryCache.TryGetValue(cacheKey, out BookContainer<Book> cacheValue))
        {
            cacheValue = _bookInfoService.GetBookList(category, serie);

            var cacheEntryOptions = new MemoryCacheEntryOptions()
                .SetSlidingExpiration(TimeSpan.FromHours(1));

            _memoryCache.Set(cacheKey, cacheValue, cacheEntryOptions);
        }

        return cacheValue;
    }

    // [HttpGet("{category}/{book}/{chapter}/{page}")]
    // public PageDetail Get(string category, string book, string chapter, int page)
    // {
    //     return _bookInfoService.GetDetail(category, book, chapter, page);
    // }
}

