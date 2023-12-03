using Microsoft.Extensions.Caching.Memory;

namespace Comicsbox
{
    public class FileMapService
    {
        private readonly IMemoryCache _memoryCache;
        private readonly IConfiguration _configuration;

        private DateTime _lastAccessTimeUtc;

        public FileMapService(IMemoryCache memoryCache, IConfiguration configuration)
        {
            _memoryCache = memoryCache;
            _configuration = configuration;
        }

        public void Init()
        {
            GetFileInfoMapAsync(true).GetAwaiter().GetResult();
        }

        public async Task<FileInfo[]> GetFileInfoMapAsync(bool allowRefreshCache = false)
        {
            string cacheKey = "fileMap";

            string basePath = _configuration.GetValue<string>("Settings:AbsoluteBasePath")!;

            var lastAccessTimeUtc = new FileInfo(Path.Combine(basePath, "LastAccessTimeUtc.txt")).LastAccessTimeUtc;

            bool resetCache = false;

            if (allowRefreshCache && _lastAccessTimeUtc != lastAccessTimeUtc)
            {
                Console.WriteLine("FileMap: Disk access time changed, reloading file map...");
                resetCache = true;
                _lastAccessTimeUtc = lastAccessTimeUtc;
            }

            if (resetCache || !_memoryCache.TryGetValue(cacheKey, out FileInfo[]? cacheValue))
            {
                DateTime now = DateTime.UtcNow;
                Console.WriteLine("FileMap: Cache is updating...");
                cacheValue = await Task.Run(() => Directory.GetFiles(basePath, "*.pdf", SearchOption.AllDirectories).Select(f => new FileInfo(f)).OrderBy(f => f.FullName).ToArray());
                Console.WriteLine($"FileMap: Cache is updated  in {(DateTime.UtcNow - now).Seconds} seconds.");

                _memoryCache.Set(cacheKey, cacheValue);
            }

            return cacheValue!;
        }

        public async Task<string[]> GetDirectoryMapAsync(bool allowRefreshCache = false)
        {
            var files = await GetFileInfoMapAsync(allowRefreshCache);
            return files.Select(f => f.DirectoryName).Distinct().ToArray()!;
        }
    }
}