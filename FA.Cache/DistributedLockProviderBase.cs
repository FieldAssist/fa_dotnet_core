// Copyright (c) FieldAssist. All Rights Reserved.

using System.Diagnostics;

namespace FA.Cache
{
    /// <summary>
    /// Abstract base class for distributed lock providers.
    /// Implements the wait/retry polling logic for <see cref="TryAcquire"/> and <see cref="TryAcquireAsync"/>.
    /// Concrete implementations only need to implement the core lock primitives.
    /// </summary>
    public abstract class DistributedLockProviderBase : IDistributedLockProvider
    {
        /// <inheritdoc />
        public abstract bool Acquire(string lockKey, string lockValue, TimeSpan expiresIn);

        /// <inheritdoc />
        public abstract Task<bool> AcquireAsync(string lockKey, string lockValue, TimeSpan expiresIn);

        /// <inheritdoc />
        public abstract bool Release(string lockKey, string lockValue);

        /// <inheritdoc />
        public abstract Task<bool> ReleaseAsync(string lockKey, string lockValue);

        /// <inheritdoc />
        public virtual IDistributedLock? TryAcquire(
            string lockKey,
            TimeSpan expiresIn,
            TimeSpan? waitTime = null,
            TimeSpan? retryInterval = null)
        {
            var lockValue = Guid.NewGuid().ToString();
            var limit = waitTime ?? TimeSpan.Zero;
            var interval = retryInterval ?? TimeSpan.FromMilliseconds(100);
            var stopwatch = Stopwatch.StartNew();

            while (true)
            {
                if (Acquire(lockKey, lockValue, expiresIn))
                {
                    return new DistributedLockHandle(this, lockKey, lockValue, true);
                }

                if (stopwatch.Elapsed >= limit)
                {
                    break;
                }

                Thread.Sleep(interval);
            }

            return null;
        }

        /// <inheritdoc />
        public virtual async Task<IDistributedLock?> TryAcquireAsync(
            string lockKey,
            TimeSpan expiresIn,
            TimeSpan? waitTime = null,
            TimeSpan? retryInterval = null,
            CancellationToken cancellationToken = default)
        {
            var lockValue = Guid.NewGuid().ToString();
            var limit = waitTime ?? TimeSpan.Zero;
            var interval = retryInterval ?? TimeSpan.FromMilliseconds(100);
            var stopwatch = Stopwatch.StartNew();

            while (true)
            {
                cancellationToken.ThrowIfCancellationRequested();

                if (await AcquireAsync(lockKey, lockValue, expiresIn))
                {
                    return new DistributedLockHandle(this, lockKey, lockValue, true);
                }

                if (stopwatch.Elapsed >= limit)
                {
                    break;
                }

                try
                {
                    await Task.Delay(interval, cancellationToken);
                }
                catch (OperationCanceledException)
                {
                    break;
                }
            }

            return null;
        }
    }
}
