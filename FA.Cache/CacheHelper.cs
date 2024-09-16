// Copyright (c) FieldAssist. All Rights Reserved.

using System.Collections.Concurrent;
using Newtonsoft.Json;

namespace FA.Cache
{
    public class CacheHelper
    {
        private readonly ICacheProvider _cacheProvider;

        private static readonly ConcurrentDictionary<string, SemaphoreSlim> s_locks = new();

        public CacheHelper(ICacheProvider cacheProvider)
        {
            _cacheProvider = cacheProvider;
        }

        public async Task<T> GetResult<T>(string cacheKey, TimeSpan expiresIn, Func<Task<T>> fetchDataFunc)
        {
            if (!_cacheProvider.TryGet(cacheKey, out T result))
            {
                var cacheLock = s_locks.GetOrAdd(cacheKey, new SemaphoreSlim(1, 1));

                await cacheLock.WaitAsync();
                try
                {
                    // Double-check the cache inside the lock
                    if (!_cacheProvider.TryGet(cacheKey, out result))
                    {
                        result = await fetchDataFunc();
                        if (result != null && !result.Equals(default(T)))
                        {
                            _cacheProvider.Insert(cacheKey, result, expiresIn);
                        }
                    }
                }
                finally
                {
                    cacheLock.Release();
                    s_locks.TryRemove(cacheKey, out _); // Optionally remove the lock after it's done
                }
            }
            else
            {
                Console.WriteLine($"Cache: 📁 Retrieved from cache -------");
                Console.WriteLine($"Cache: 📁 Key: {cacheKey}");
            }

            // var data = JsonConvert.SerializeObject(result);
            // if (data.Contains("Error"))
            // {
            //     // Catch Redis Error and return data through DB call
            //     result = await fetchDataFunc();
            // }

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
