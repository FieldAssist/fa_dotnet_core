using Microsoft.Extensions.Caching.Memory;

namespace FA.Cache.Providers
{
    public class MemoryCacheProvider : ICacheProvider
    {
        private readonly IMemoryCache cache;

        public MemoryCacheProvider(IMemoryCache cache)
        {
            this.cache = cache;
        }

        public void Initialize()
        {
            // TODO: do nothing
        }

        /// <summary>
        /// does not support expiration
        /// </summary>
        /// <param name="cacheKey"></param>
        /// <param name="value"></param>
        /// <param name="expiresIn">expiration</param>
        public void Insert<T>(string cacheKey, T value, TimeSpan expiresIn)
        {
            cache.Set(cacheKey, value, expiresIn);
        }

        public bool TryGet<T>(string cacheKey, out T result)
        {
            result = default(T);
            if (cache.TryGetValue(cacheKey, out var v))
            {
                if (v is T t)
                {
                    result = t;
                    return true;
                }
            }

            return false;
        }

        public void TryRemove(string cacheKey)
        {
            try
            {
                cache.Remove(cacheKey);
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine(ex);
            }
        }
    }
}