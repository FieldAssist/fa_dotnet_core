// Copyright (c) FieldAssist. All Rights Reserved.

using Microsoft.Extensions.Logging;

namespace FA.Cache
{
    public class CacheHelper
    {
        private readonly ICacheProvider _cacheProvider;
        private readonly ILogger<CacheHelper> _logger;

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

            var result = await fetchDataFunc();
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

        public async Task<(bool IsAllowed, int CurrentCount)> IncrementWithLimitAsync(string cacheKey, int maxAllowed, TimeSpan expiresIn)
        {
            try
            {
                var (found, currentValue) = await _cacheProvider.TryGetAsync<int>(cacheKey);
                int newValue;
                if (!found)
                {
                    newValue = 1;
                }
                else
                {
                    newValue = currentValue + 1;
                }

                if (newValue > maxAllowed)
                {
                    _logger.LogWarning($"Cache: :no_entry_sign: Rate limit hit for key {cacheKey}. Count={newValue}");
                    return (false, currentValue);
                }

                _cacheProvider.Insert(cacheKey, newValue, expiresIn);
                _logger.LogInformation($"Cache: :1234: Incremented key {cacheKey} → {newValue}");
                return (true, newValue);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Cache rate limit failed for key {cacheKey}");
                return (true, 0);
            }
        }
    }
}
