// Copyright (c) FieldAssist. All Rights Reserved.

using Newtonsoft.Json;
using StackExchange.Redis;

namespace FA.Cache.Providers
{
    public class RedisCacheProvider : ICacheProvider
    {
        private readonly IDatabase _cache;
        private readonly IServer _server;

        public RedisCacheProvider(string redisConnectionString)
        {
            Console.WriteLine("RedisCacheProvider: Setting up redis connection...");
            var redis = ConnectionMultiplexer.Connect(ConfigurationOptions.Parse(redisConnectionString));
            // Assuming only one server
            _server = redis.GetServers().FirstOrDefault();
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

        /// <summary>
        /// Removes keys on the basis of pattern.<br/>
        /// TCP call for each key.
        /// </summary>
        /// <param name="pattern">Pattern according to which the keys are filtered and removed.</param>
        public void TryRemoveAllKeysByPattern(string pattern)
        {
            if (_server is not null)
            {
                foreach (var key in _server.Keys(pattern: pattern))
                {
                    _cache.KeyDelete(key);
                }
            }
        }

        /// <summary>
        /// Removes keys on the basis of pattern using LUA.<br/>
        /// Single TCP call for all keys.
        /// </summary>
        /// <param name="pattern">Pattern according to which the keys are filtered and removed.</param>
        public void TryRemoveAllKeysByPatternUsingLua(string pattern)
        {
            var script = @"local keys = redis.call('KEYS', ARGV[1])
                           for _, key in ipairs(keys) do
                               redis.call('DEL', key)
                           end";

            _cache.ScriptEvaluate(script, null, new RedisValue[] { pattern }, CommandFlags.FireAndForget);
        }
    }
}
