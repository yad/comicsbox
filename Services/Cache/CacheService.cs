using Microsoft.Extensions.Caching.Memory;

namespace Comicsbox.Cache
{
    public interface ICacheService
    {
        T TryLoadFromCache<T>(string key, Func<T> service) where T : ICacheConfiguration;
    }

    public class CacheService : ICacheService
    {
        private readonly IMemoryCache _memoryCache;

        public CacheService(IMemoryCache memoryCache)
        {
            _memoryCache = memoryCache;
        }

        public T TryLoadFromCache<T>(string key, Func<T> service) where T : ICacheConfiguration
        {
            T result;
            if (!_memoryCache.TryGetValue(key, out result))
            {
                result = service();
                if (result.CacheResult)
                {
                    _memoryCache.Set(key, result);
                }
            }

            return result;
        }
    }
}
