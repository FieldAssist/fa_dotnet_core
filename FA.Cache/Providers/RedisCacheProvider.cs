// Copyright (c) FieldAssist. All Rights Reserved.

using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using StackExchange.Redis;

namespace FA.Cache.Providers
{
    public class RedisCacheProvider : ICacheProvider
    {
        private readonly ILogger<RedisCacheProvider> _logger;
        private readonly IDatabase _cache;
        private readonly IServer _server;

        public RedisCacheProvider(ILogger<RedisCacheProvider> logger, string redisConnectionString)
        {
            _logger = logger;
            _logger.LogInformation("RedisCacheProvider: Setting up redis connection...");
            var redis = ConnectionMultiplexer.Connect(ConfigurationOptions.Parse(redisConnectionString));
            // Assuming only one server
            _server = redis.GetServers().FirstOrDefault();
            _cache = redis.GetDatabase();
            _logger.LogInformation("RedisCacheProvider: Redis connected successfully");
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
                _logger.LogError(ex, ex.Message);
            }
        }

        public bool TryGet<T>(string cacheKey, out T result)
        {
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
                _logger.LogError($"Error fetching cache key {cacheKey}: {ex.Message}");
                result = default(T); // Reset result to default in case of error
            }

            return false;
        }

        public async Task<(bool isSuccess, T result)> TryGetAsync<T>(string cacheKey)
        {
            var result = default(T);
            try
            {
                var value = await _cache.StringGetAsync(cacheKey); // Asynchronous call to Redis
                if (!value.IsNull)
                {
                    result = JsonConvert.DeserializeObject<T>(value); // Deserialize value if found
                    return (true, result);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error fetching cache key {cacheKey}: {ex.Message}");
            }

            return (false, default(T)); // Return false and default result in case of failure
        }

        public void TryRemove(string cacheKey)
        {
            try
            {
                _cache.KeyDelete(cacheKey);
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