// Copyright (c) FieldAssist. All Rights Reserved.

using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using StackExchange.Redis;

namespace FA.Cache.Providers
{
    public class RedisReadWriteCacheProvider : ICacheProvider
    {
        private readonly ILogger<RedisReadWriteCacheProvider> _logger;
        private readonly IDatabase _readCache;
        private readonly IDatabase _redisWrite;
        private readonly IServer _serverWtite;

        public RedisReadWriteCacheProvider(ILogger<RedisReadWriteCacheProvider> logger,
            string redisReadOnlyConnectionString, string redisWriteOnlyConnectionString)
        {
            _logger = logger;
            _logger.LogInformation($"Setting up redis connection...");

            var redisRead = ConnectionMultiplexer.Connect(ConfigurationOptions.Parse(redisReadOnlyConnectionString));
            _readCache = redisRead.GetDatabase();
            _logger.LogInformation($"Redis read only connected successfully");

            var redisWrite = ConnectionMultiplexer.Connect(ConfigurationOptions.Parse(redisWriteOnlyConnectionString));
            // Assuming only one server
            _serverWtite = redisWrite.GetServers().FirstOrDefault();
            _redisWrite = redisWrite.GetDatabase();
            _logger.LogInformation($"Redis write only connected successfully");
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
                _logger.LogError(ex, ex.Message);
            }
        }

        public bool TryGet<T>(string cacheKey, out T result)
        {
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
                _logger.LogError(ex, $"Error fetching cache key {cacheKey}: {ex.Message}");
                result = default(T); // Reset result to default in case of error
            }

            return false;
        }

        public async Task<(bool isSuccess, T result)> TryGetAsync<T>(string cacheKey)
        {
            var result = default(T);
            try
            {
                var value = await _readCache.StringGetAsync(cacheKey);
                if (!value.IsNull)
                {
                    result = JsonConvert.DeserializeObject<T>(value);
                    return (true, result);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error fetching cache key {cacheKey}: {ex.Message}");
            }

            return (false, default(T)); // Return false and default value if operation fails
        }

        public void TryRemove(string cacheKey)
        {
            try
            {
                _redisWrite.KeyDelete(cacheKey);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message);
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