using Microsoft.Extensions.Caching.Memory;

namespace Comicsbox
{
    public class FileMapService
    {
        private readonly IMemoryCache _memoryCache;
        private readonly IConfiguration _configuration;

        private string _checksum = "replaceme";

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

            var checksum = File.ReadAllText(Path.Combine(basePath, "checksum"));

            bool resetCache = false;

            if (allowRefreshCache && _checksum != checksum)
            {
                Console.WriteLine("FileMap: Disk access time changed, reloading file map...");
                resetCache = true;
                _checksum = checksum;
            }

            if (resetCache || !_memoryCache.TryGetValue(cacheKey, out FileInfo[]? cacheValue))
            {
                DateTime now = DateTime.UtcNow;
                Console.WriteLine("FileMap: Cache is updating...");
                cacheValue = await Task.Run(() => Directory.GetFiles(basePath, "*.pdf", SearchOption.AllDirectories).Select(f => new FileInfo(f)).OrderBy(f => f.FullName).ToArray());
                Console.WriteLine($"FileMap: Cache updated in {(DateTime.UtcNow - now).Seconds} seconds.");

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