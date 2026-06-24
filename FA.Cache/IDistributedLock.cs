// Copyright (c) FieldAssist. All Rights Reserved.

namespace FA.Cache
{
    /// <summary>
    /// Represents an acquired distributed lock handle.
    /// Disposing the handle will release the lock.
    /// </summary>
    public interface IDistributedLock : IAsyncDisposable, IDisposable
    {
        /// <summary>
        /// The key used to identify the lock.
        /// </summary>
        string Key { get; }

        /// <summary>
        /// The unique value associated with this lock acquisition.
        /// Used to ensure only the owner can release the lock.
        /// </summary>
        string Value { get; }

        /// <summary>
        /// Whether the lock was successfully acquired.
        /// </summary>
        bool IsAcquired { get; }
    }
}
