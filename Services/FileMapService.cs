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

        public async Task<string[]> GetFileMapAsync()
        {
            string cacheKey = "fileMap";

            string basePath = _configuration.GetValue<string>("Settings:AbsoluteBasePath")!;

            var lastAccessTimeUtc = new FileInfo(basePath).LastAccessTimeUtc;

            if (_lastAccessTimeUtc != lastAccessTimeUtc)
            {
                Console.WriteLine("Disk access time changed, reloading file map...");
                _memoryCache.Remove(cacheKey);
                _lastAccessTimeUtc = lastAccessTimeUtc;
            }

            if (!_memoryCache.TryGetValue(cacheKey, out string[] cacheValue))
            {
                // "lock" avoid multiple caching
                _memoryCache.Set(cacheKey, Array.Empty<string>());

                Console.WriteLine("File map is caching ...");
                cacheValue = await Task.Run(() => Directory.GetFiles(basePath, "*.pdf", SearchOption.AllDirectories).Order().ToArray());

                _memoryCache.Set(cacheKey, cacheValue);
            }

            return cacheValue;
        }

        public async Task<string[]> GetDirectoryMapAsync()
        {
            var files = await GetFileMapAsync();
            return files.Select(Path.GetDirectoryName).Distinct().ToArray()!;
        }
    }
}