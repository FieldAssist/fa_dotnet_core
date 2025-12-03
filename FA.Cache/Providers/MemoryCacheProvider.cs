// Copyright (c) FieldAssist. All Rights Reserved.

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

        public async Task<(bool isSuccess, T result)> TryGetAsync<T>(string cacheKey)
        {
            var result = default(T);

            // Simulate asynchronous execution with Task.FromResult
            var cacheResult = await Task.FromResult(cache.TryGetValue(cacheKey, out var v));

            if (cacheResult)
            {
                if (v is T t)
                {
                    result = t;
                    return (true, result);
                }
            }

            return (false, default(T)); // Return false and default if not found or type mismatch
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

        /// <summary>
        /// NO IMPLEMENTATION FOR MEMORY CACHE.
        /// </summary>
        /// <param name="pattern"></param>
        public void TryRemoveAllKeysByPattern(string pattern)
        {
        }

        /// <summary>
        /// NO IMPLEMENTATION FOR MEMORY CACHE.
        /// </summary>
        /// <param name="pattern"></param>
        public void TryRemoveAllKeysByPatternUsingLua(string pattern)
        {
        }
    }
}
