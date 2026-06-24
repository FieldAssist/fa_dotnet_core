// Copyright (c) FieldAssist. All Rights Reserved.

using Microsoft.Extensions.Logging;
using StackExchange.Redis;

namespace FA.Cache.Providers
{
    /// <summary>
    /// Redis-based distributed lock provider using StackExchange.Redis LockTake/LockRelease.
    /// Compatible with Redis and Dragonfly DB (uses standard SET NX PX and Lua script internally).
    /// </summary>
    public class RedisDistributedLockProvider : DistributedLockProviderBase
    {
        private readonly IDatabase _database;
        private readonly ILogger<RedisDistributedLockProvider> _logger;

        public RedisDistributedLockProvider(ILogger<RedisDistributedLockProvider> logger, IDatabase database)
        {
            _logger = logger;
            _database = database;
        }

        /// <inheritdoc />
        public override bool Acquire(string lockKey, string lockValue, TimeSpan expiresIn)
        {
            try
            {
                return _database.LockTake(lockKey, lockValue, expiresIn);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to acquire lock for key {LockKey}", lockKey);
                return false;
            }
        }

        /// <inheritdoc />
        public override async Task<bool> AcquireAsync(string lockKey, string lockValue, TimeSpan expiresIn)
        {
            try
            {
                return await _database.LockTakeAsync(lockKey, lockValue, expiresIn);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to acquire lock for key {LockKey} asynchronously", lockKey);
                return false;
            }
        }

        /// <inheritdoc />
        public override bool Release(string lockKey, string lockValue)
        {
            try
            {
                return _database.LockRelease(lockKey, lockValue);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to release lock for key {LockKey}", lockKey);
                return false;
            }
        }

        /// <inheritdoc />
        public override async Task<bool> ReleaseAsync(string lockKey, string lockValue)
        {
            try
            {
                return await _database.LockReleaseAsync(lockKey, lockValue);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to release lock for key {LockKey} asynchronously", lockKey);
                return false;
            }
        }
    }
}
