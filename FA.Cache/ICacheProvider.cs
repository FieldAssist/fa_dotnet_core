namespace FA.Cache
{
    public interface ICacheProvider
    {
        void Initialize();
        void Insert<T>(string cacheKey, T result, TimeSpan expiresIn);
        bool TryGet<T>(string cacheKey, out T result);
        void TryRemove(string cacheKey);
    }
}