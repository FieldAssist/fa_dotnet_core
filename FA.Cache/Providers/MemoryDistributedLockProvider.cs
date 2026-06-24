// Copyright (c) FieldAssist. All Rights Reserved.

using System.Collections.Concurrent;

namespace FA.Cache.Providers
{
    /// <summary>
    /// Thread-safe, in-memory distributed lock provider.
    /// Useful for local development and testing when Redis is not available.
    /// Note: This only provides locking within a single process and is NOT distributed.
    /// </summary>
    public class MemoryDistributedLockProvider : DistributedLockProviderBase
    {
        private readonly ConcurrentDictionary<string, (string Value, DateTime ExpiresAt)> _locks = new();

        /// <inheritdoc />
        public override bool Acquire(string lockKey, string lockValue, TimeSpan expiresIn)
        {
            var now = DateTime.UtcNow;
            var expiresAt = now.Add(expiresIn);

            // Atomic compare-and-swap loop
            while (true)
            {
                if (_locks.TryGetValue(lockKey, out var currentLock))
                {
                    // Lock exists — check if it has expired
                    if (currentLock.ExpiresAt < now)
                    {
                        // Expired lock: try to replace it atomically
                        if (_locks.TryUpdate(lockKey, (lockValue, expiresAt), currentLock))
                        {
                            return true;
                        }

                        // Another thread beat us — retry
                        continue;
                    }

                    // Lock is still held by someone else
                    return false;
                }
                else
                {
                    // No lock exists — try to add one atomically
                    if (_locks.TryAdd(lockKey, (lockValue, expiresAt)))
                    {
                        return true;
                    }

                    // Another thread beat us — retry
                    continue;
                }
            }
        }

        /// <inheritdoc />
        public override Task<bool> AcquireAsync(string lockKey, string lockValue, TimeSpan expiresIn)
        {
            return Task.FromResult(Acquire(lockKey, lockValue, expiresIn));
        }

        /// <inheritdoc />
        public override bool Release(string lockKey, string lockValue)
        {
            if (_locks.TryGetValue(lockKey, out var currentLock))
            {
                if (currentLock.Value == lockValue)
                {
                    return _locks.TryRemove(lockKey, out _);
                }
            }

            return false;
        }

        /// <inheritdoc />
        public override Task<bool> ReleaseAsync(string lockKey, string lockValue)
        {
            return Task.FromResult(Release(lockKey, lockValue));
        }
    }
}
