// Copyright (c) FieldAssist. All Rights Reserved.

namespace FA.Cache
{
    /// <summary>
    /// A disposable lock handle that automatically releases the lock when disposed.
    /// Thread-safe: the lock is released at most once even if Dispose is called multiple times.
    /// </summary>
    public class DistributedLockHandle : IDistributedLock
    {
        private readonly IDistributedLockProvider _provider;
        private int _disposed;

        /// <inheritdoc />
        public string Key { get; }

        /// <inheritdoc />
        public string Value { get; }

        /// <inheritdoc />
        public bool IsAcquired { get; }

        public DistributedLockHandle(IDistributedLockProvider provider, string key, string value, bool isAcquired)
        {
            _provider = provider;
            Key = key;
            Value = value;
            IsAcquired = isAcquired;
        }

        /// <summary>
        /// Synchronously releases the lock if it was acquired and not already released.
        /// </summary>
        public void Dispose()
        {
            if (Interlocked.CompareExchange(ref _disposed, 1, 0) == 0)
            {
                if (IsAcquired)
                {
                    _provider.Release(Key, Value);
                }
            }

            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Asynchronously releases the lock if it was acquired and not already released.
        /// </summary>
        public async ValueTask DisposeAsync()
        {
            if (Interlocked.CompareExchange(ref _disposed, 1, 0) == 0)
            {
                if (IsAcquired)
                {
                    await _provider.ReleaseAsync(Key, Value);
                }
            }

            GC.SuppressFinalize(this);
        }
    }
}
