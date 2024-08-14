﻿// Copyright (c) FieldAssist. All Rights Reserved.

using Newtonsoft.Json;

namespace FA.Cache
{
    public class CacheHelper
    {
        private readonly ICacheProvider _cacheProvider;

        public CacheHelper(ICacheProvider cacheProvider)
        {
            _cacheProvider = cacheProvider;
        }

        public async Task<T> GetResult<T>(string cacheKey, TimeSpan expiresIn,
            Func<Task<T>> fetchDataFunc)
        {
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
                // Console.WriteLine($"Cache: 📁 Result: {result}");
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