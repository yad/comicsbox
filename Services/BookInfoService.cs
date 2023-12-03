using Microsoft.Extensions.Caching.Memory;

namespace Comicsbox
{
    public class BookInfoService
    {
        private readonly IMemoryCache _memoryCache;
        private readonly IConfiguration _configuration;
        private readonly FileMapService _fileMapService;
        private readonly ThumbnailProvider _thumbnailProvider;

        public BookInfoService(IMemoryCache memoryCache, IConfiguration configuration, FileMapService fileMapService, ThumbnailProvider thumbnailProvider)
        {
            _memoryCache = memoryCache;
            _configuration = configuration;
            _fileMapService = fileMapService;
            _thumbnailProvider = thumbnailProvider;
        }

        public async Task<BookContainer<Book>> GetBookListAsync(string category, string serie = "", bool resetCache = false)
        {
            var cacheKey = $"{category}-{serie}";
            if (resetCache || !_memoryCache.TryGetValue(cacheKey, out BookContainer<Book>? cacheValue))
            {
                cacheValue = await BuildBookInfoAsync(category, serie);

                var cacheEntryOptions = new MemoryCacheEntryOptions()
                    .SetSlidingExpiration(TimeSpan.FromHours(1));

                _memoryCache.Set(cacheKey, cacheValue, cacheEntryOptions);
            }

            return cacheValue!;
        }

        public string GetSeriePath(string category, string serie)
        {
            var basePath = _configuration.GetValue<string>("Settings:AbsoluteBasePath")!;
            return Path.Combine(basePath, category, serie);
        }

        public string GetSerieBookPath(string category, string serie, string book)
        {
            var basePath = _configuration.GetValue<string>("Settings:AbsoluteBasePath")!;
            return Path.Combine(basePath, category, serie, $"{book}.pdf");
        }

        private async Task<BookContainer<Book>> BuildBookInfoAsync(string category, string serie)
        {
            var path = GetSeriePath(category, serie);

            var files = await _fileMapService.GetFileInfoMapAsync();

            List<Book> books = new List<Book>();

            if (string.IsNullOrEmpty(serie))
            {
                foreach (var file in files)
                {
                    var currentCategory = Path.GetFileName(Path.GetDirectoryName(file.DirectoryName))!;
                    var currentSerie = Path.GetFileName(file.DirectoryName)!;

                    if (category == currentCategory && !books.Any(b => b.Name == currentSerie))
                    {
                        var thumbnail = _thumbnailProvider.GetThumbnailFileName(file.FullName);
                        books.Add(new Book(currentSerie, thumbnail));
                    }
                }
            }
            else
            {
                foreach (var file in files)
                {
                    var currentCategory = Path.GetFileName(Path.GetDirectoryName(file.DirectoryName))!;
                    var currentSerie = Path.GetFileName(file.DirectoryName)!;

                    if (category == currentCategory && serie == currentSerie)
                    {
                        var book = Path.GetFileNameWithoutExtension(file.FullName);
                        var thumbnail = _thumbnailProvider.GetThumbnailFileName(file.FullName);
                        books.Add(new Book(book, thumbnail));
                    }
                }
            }

            return new BookContainer<Book>("", books.OrderBy(b => b.Name));
        }
    }
}
