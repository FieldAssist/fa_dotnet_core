// Copyright (c) FieldAssist. All Rights Reserved.

using System.Collections.Concurrent;
using Microsoft.Extensions.Logging;

namespace FA.Cache
{
    public class CacheHelper
    {
        private readonly ICacheProvider _cacheProvider;
        private readonly ILogger<CacheHelper> _logger;

        private static readonly ConcurrentDictionary<string, SemaphoreSlim> s_locks = new();

        public CacheHelper(ILogger<CacheHelper> logger, ICacheProvider cacheProvider)
        {
            _cacheProvider = cacheProvider;
            _logger = logger;
        }

        public async Task<T> GetResult<T>(string cacheKey, TimeSpan expiresIn, Func<Task<T>> fetchDataFunc)
        {
            // Try to get the result from the cache
            if (_cacheProvider.TryGet(cacheKey, out T cachedResult))
            {
                _logger.LogInformation($"Cache: 📁 Retrieved from cache. Key: {cacheKey}");
                return cachedResult;
            }

            // Fetch the data from the external source (DB, etc.) using the provided callback
            var result = await fetchDataFunc();

            // If the result is valid, cache it and return
            if (result != null && !result.Equals(default(T)))
            {
                _cacheProvider.Insert(cacheKey, result, expiresIn);
            }

            return result;
        }

        public async Task<T> GetResultAsync<T>(string cacheKey, TimeSpan expiresIn, Func<Task<T>> fetchDataFunc)
        {
            // First check without locking
            var (isSuccess1, cachedResult) = await _cacheProvider.TryGetAsync<T>(cacheKey);
            if (isSuccess1)
            {
                _logger.LogInformation($"Cache: 📁 Retrieved from cache. Key: {cacheKey}");
                return cachedResult;
            }

            var result = await fetchDataFunc();
            if (result != null && !result.Equals(default(T)))
            {
                _cacheProvider.Insert(cacheKey, result, expiresIn);
            }

            return result;
        }


        [Obsolete("Use RemoveKey instead")]
        public void RemoveCacheKey(string cacheKey)
        {
            _cacheProvider.TryRemove(cacheKey);
        }

        public void RemoveKey(string cacheKey)
        {
            _cacheProvider.TryRemove(cacheKey);
        }

        public void RemoveKeys(string pattern)
        {
            _cacheProvider.TryRemoveAllKeysByPattern(pattern);
        }

        public void RemoveSmallCountOfKeys(string pattern)
        {
            _cacheProvider.TryRemoveAllKeysByPattern(pattern);
        }

        public void RemoveLargeCountOfKeys(string pattern)
        {
            _cacheProvider.TryRemoveAllKeysByPatternUsingLua(pattern);
        }
    }
}
