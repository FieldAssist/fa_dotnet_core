using Newtonsoft.Json;
using StackExchange.Redis;

namespace FA.Cache.Providers
{
    public class RedisCacheProvider : ICacheProvider
    {
        private readonly IDatabase _cache;

        public RedisCacheProvider(string redisConnectionString)
        {
            Console.WriteLine("RedisCacheProvider: Setting up redis connection...");
            var redis = ConnectionMultiplexer.Connect(ConfigurationOptions.Parse(redisConnectionString));
            _cache = redis.GetDatabase();
            Console.WriteLine("RedisCacheProvider: Redis connected successfully");
            // this.errorMessenger = errorMessenger;
            // this.myLogger = myLogger;
        }

        public void Initialize()
        {
            // TODO: do nothing
        }

        /// <summary>
        /// supports Expiration
        /// </summary>
        /// <param name="cacheKey"></param>
        /// <param name="result"></param>
        /// <param name="expiresIn">Time after which Key will be removed</param>
        public void Insert<T>(string cacheKey, T result, TimeSpan expiresIn)
        {
            try
            {
                var modelS = JsonConvert.SerializeObject(result);
                _cache.StringSet(cacheKey, modelS, expiresIn);
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine(ex);
                // Task.Run(async () => { await myLogger.SaveLog(ex, $"Cache Key: {cacheKey}", ex.StackTrace ?? ""); });
            }
        }

        public bool TryGet<T>(string cacheKey, out T result)
        {
            ThreadPool.SetMinThreads(200, 2);
            result = default(T);
            try
            {
                var value = _cache.StringGet(cacheKey);
                if (!value.IsNull)
                {
                    result = JsonConvert.DeserializeObject<T>(value);
                    return true;
                }
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine(ex);
                return true;
            }

            return false;
        }

        public void TryRemove(string cacheKey)
        {
            try
            {
                _cache.KeyDelete(cacheKey);
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine(ex);
            }
        }
    }
}