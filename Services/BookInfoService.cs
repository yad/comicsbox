using Microsoft.Extensions.Caching.Memory;

namespace Comicsbox
{
    public class BookInfoService
    {
        private readonly IMemoryCache _memoryCache;
        private readonly IConfiguration _configuration;
        private readonly ThumbnailProvider _thumbnailProvider;

        public BookInfoService(IMemoryCache memoryCache, IConfiguration configuration, ThumbnailProvider thumbnailProvider)
        {
            _memoryCache = memoryCache;
            _configuration = configuration;
            _thumbnailProvider = thumbnailProvider;
        }

        public BookContainer<Book> GetBookList(string category, string serie = "", bool resetCache = false)
        {
            var cacheKey = $"{category}-{serie}";
            if (resetCache || !_memoryCache.TryGetValue(cacheKey, out BookContainer<Book> cacheValue))
            {
                cacheValue = BuildBookInfo(category, serie);

                var cacheEntryOptions = new MemoryCacheEntryOptions()
                    .SetSlidingExpiration(TimeSpan.FromHours(1));

                _memoryCache.Set(cacheKey, cacheValue, cacheEntryOptions);
            }

            return cacheValue;
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

        private BookContainer<Book> BuildBookInfo(string category, string serie)
        {
            var path = GetSeriePath(category, serie);

            List<Book> books = new List<Book>();

            if (string.IsNullOrEmpty(serie))
            {
                foreach (var dir in Directory.GetDirectories(path))
                {
                    serie = Path.GetFileName(dir);
                    var file = Directory.GetFiles(dir).Order().FirstOrDefault(f => Path.GetExtension(f) == ".pdf");
                    var thumbnail = "";
                    if (file != null)
                    {
                        thumbnail = _thumbnailProvider.GetThumbnailFileName(file);
                    }

                    books.Add(new Book(serie, thumbnail));
                }
            }
            else
            {
                foreach (var file in Directory.GetFiles(path))
                {
                    var book = Path.GetFileNameWithoutExtension(file);
                    var thumbnail = _thumbnailProvider.GetThumbnailFileName(file);
                    books.Add(new Book(book, thumbnail));
                }
            }

            return new BookContainer<Book>("", books.OrderBy(b => b.Name));
        }
    }
}
