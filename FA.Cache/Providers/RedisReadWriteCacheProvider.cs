// Copyright (c) FieldAssist. All Rights Reserved.

using Newtonsoft.Json;
using StackExchange.Redis;

namespace FA.Cache.Providers
{
    public class RedisReadWriteCacheProvider : ICacheProvider
    {
        private const string Tag = nameof(RedisReadWriteCacheProvider);
        private readonly IDatabase _readCache;
        private readonly IDatabase _redisWrite;
        private readonly IServer _serverWtite;

        public RedisReadWriteCacheProvider(string redisReadOnlyConnectionString, string redisWriteOnlyConnectionString)
        {
            Console.WriteLine($"{Tag}: Setting up redis connection...");

            var redisRead = ConnectionMultiplexer.Connect(ConfigurationOptions.Parse(redisReadOnlyConnectionString));
            _readCache = redisRead.GetDatabase();
            Console.WriteLine($"{Tag}: Redis read only connected successfully");

            var redisWrite = ConnectionMultiplexer.Connect(ConfigurationOptions.Parse(redisWriteOnlyConnectionString));
            // Assuming only one server
            _serverWtite = redisWrite.GetServers().FirstOrDefault();
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

        public void TryRemove(string cacheKey)
        {
            try
            {
                _redisWrite.KeyDelete(cacheKey);
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
            if (_serverWtite is not null)
            {
                foreach (var key in _serverWtite.Keys(pattern: pattern))
                {
                    _redisWrite.KeyDelete(key);
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

            _redisWrite.ScriptEvaluate(script, null, new RedisValue[] { pattern }, CommandFlags.FireAndForget);
        }
    }
}
