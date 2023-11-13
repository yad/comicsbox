﻿using Microsoft.AspNetCore.Mvc;

namespace Comicsbox.Controllers;

[ApiController]
[Route("[controller]")]
public class ThumbnailController : ControllerBase
{
    private readonly BookInfoService _bookInfoService;

    public ThumbnailController(BookInfoService bookInfoService)
    {
        _bookInfoService = bookInfoService;
    }

    [HttpGet("{category}/{pagination}")]
    public BookContainer<Book> Get(string category, int pagination)
    {
        return _bookInfoService.GetBookThumbnails(category, "").WithPagination(pagination);
    }

    [HttpGet("{category}/{book}/{pagination}")]
    public BookContainer<Book> Get(string category, string book, int pagination)
    {
        return _bookInfoService.GetBookThumbnails(category, book).WithPagination(pagination);
    }
}