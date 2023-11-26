using Newtonsoft.Json;
using StackExchange.Redis;

namespace FA.Cache.Providers
{
    public class RedisReadWriteCacheProvider : ICacheProvider
    {
        private const string Tag = nameof(RedisReadWriteCacheProvider);
        private readonly IDatabase _readCache;
        private readonly IDatabase _redisWrite;

        public RedisReadWriteCacheProvider(string redisReadOnlyConnectionString, string redisWriteOnlyConnectionString)
        {
            Console.WriteLine($"{Tag}: Setting up redis connection...");

            var redisRead = ConnectionMultiplexer.Connect(ConfigurationOptions.Parse(redisReadOnlyConnectionString));
            _readCache = redisRead.GetDatabase();
            Console.WriteLine($"{Tag}: Redis read only connected successfully");

            var redisWrite = ConnectionMultiplexer.Connect(ConfigurationOptions.Parse(redisWriteOnlyConnectionString));
            _redisWrite = redisWrite.GetDatabase();
            Console.WriteLine($"{Tag}: Redis write only connected successfully");
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
                _redisWrite.StringSet(cacheKey, modelS, expiresIn);
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
                var value = _readCache.StringGet(cacheKey);
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
    }
}