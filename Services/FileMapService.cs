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
            GetFileMapAsync(true).GetAwaiter().GetResult();
        }

        public async Task<string[]> GetFileMapAsync(bool allowRefreshCache = false)
        {
            string cacheKey = "fileMap";

            string basePath = _configuration.GetValue<string>("Settings:AbsoluteBasePath")!;

            var lastAccessTimeUtc = new FileInfo(basePath).LastAccessTimeUtc;

            if (allowRefreshCache && _lastAccessTimeUtc != lastAccessTimeUtc)
            {
                Console.WriteLine("FileMap: Disk access time changed, reloading file map...");
                _memoryCache.Remove(cacheKey);
                _lastAccessTimeUtc = lastAccessTimeUtc;
            }

            if (!_memoryCache.TryGetValue(cacheKey, out string[] cacheValue))
            {
                Console.WriteLine("FileMap: Cache is updating...");
                cacheValue = await Task.Run(() => Directory.GetFiles(basePath, "*.pdf", SearchOption.AllDirectories).Order().ToArray());
                Console.WriteLine("FileMap: Cache is updated.");

                _memoryCache.Set(cacheKey, cacheValue);
            }

            return cacheValue;
        }

        public async Task<string[]> GetDirectoryMapAsync(bool allowRefreshCache = false)
        {
            var files = await GetFileMapAsync(allowRefreshCache);
            return files.Select(Path.GetDirectoryName).Distinct().ToArray()!;
        }
    }
}