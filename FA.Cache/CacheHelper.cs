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
            // First check without locking
            if (_cacheProvider.TryGet(cacheKey, out T result1))
            {
                Console.WriteLine($"Cache: 📁 Retrieved from cache. Key: {cacheKey}");
                return result1;
            }

            var cacheLock = s_locks.GetOrAdd(cacheKey, new SemaphoreSlim(1, 1));
            await cacheLock.WaitAsync();
            try
            {
                // check the cache inside the lock
                if (!_cacheProvider.TryGet(cacheKey, out T result))
                {
                    result = await fetchDataFunc();
                    if (result != null && !result.Equals(default(T)))
                    {
                        _cacheProvider.Insert(cacheKey, result, expiresIn);
                    }
                }
                else
                {
                    Console.WriteLine($"Cache: 📁 Retrieved from cache -------");
                    Console.WriteLine($"Cache: 📁 Key: {cacheKey}");
                }

                return result;
            }
            finally
            {
                cacheLock.Release();
                s_locks.TryRemove(cacheKey, out _); // Optionally remove the lock after it's done
            }


            // var data = JsonConvert.SerializeObject(result);
            // if (data.Contains("Error"))
            // {
            //     // Catch Redis Error and return data through DB call
            //     result = await fetchDataFunc();
            // }
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
