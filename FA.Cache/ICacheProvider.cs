// Copyright (c) FieldAssist. All Rights Reserved.

namespace FA.Cache
{
    public interface ICacheProvider
    {
        void Initialize();
        void Insert<T>(string cacheKey, T result, TimeSpan expiresIn);
        bool TryGet<T>(string cacheKey, out T result);
        Task<(bool isSuccess, T result)> TryGetAsync<T>(string cacheKey);
        void TryRemove(string cacheKey);
        void TryRemoveAllKeysByPattern(string pattern);
        void TryRemoveAllKeysByPatternUsingLua(string pattern);
    }
}