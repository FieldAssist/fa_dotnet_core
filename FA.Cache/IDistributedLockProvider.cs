// Copyright (c) FieldAssist. All Rights Reserved.

namespace FA.Cache
{
    /// <summary>
    /// Provides distributed locking capabilities across multiple service instances.
    /// </summary>
    public interface IDistributedLockProvider
    {
        /// <summary>
        /// Attempts to acquire a lock with the given key and value.
        /// </summary>
        /// <param name="lockKey">The key to lock on.</param>
        /// <param name="lockValue">A unique value identifying the lock owner.</param>
        /// <param name="expiresIn">The lock will automatically expire after this duration.</param>
        /// <returns>True if the lock was acquired; false otherwise.</returns>
        bool Acquire(string lockKey, string lockValue, TimeSpan expiresIn);

        /// <summary>
        /// Asynchronously attempts to acquire a lock with the given key and value.
        /// </summary>
        /// <param name="lockKey">The key to lock on.</param>
        /// <param name="lockValue">A unique value identifying the lock owner.</param>
        /// <param name="expiresIn">The lock will automatically expire after this duration.</param>
        /// <returns>True if the lock was acquired; false otherwise.</returns>
        Task<bool> AcquireAsync(string lockKey, string lockValue, TimeSpan expiresIn);

        /// <summary>
        /// Releases a previously acquired lock.
        /// Only the owner (matching lockValue) can release the lock.
        /// </summary>
        /// <param name="lockKey">The key of the lock to release.</param>
        /// <param name="lockValue">The value used when the lock was acquired.</param>
        /// <returns>True if the lock was released; false otherwise.</returns>
        bool Release(string lockKey, string lockValue);

        /// <summary>
        /// Asynchronously releases a previously acquired lock.
        /// Only the owner (matching lockValue) can release the lock.
        /// </summary>
        /// <param name="lockKey">The key of the lock to release.</param>
        /// <param name="lockValue">The value used when the lock was acquired.</param>
        /// <returns>True if the lock was released; false otherwise.</returns>
        Task<bool> ReleaseAsync(string lockKey, string lockValue);

        /// <summary>
        /// Tries to acquire a lock, optionally waiting and retrying.
        /// Returns an <see cref="IDistributedLock"/> handle that auto-releases on disposal.
        /// </summary>
        /// <param name="lockKey">The key to lock on.</param>
        /// <param name="expiresIn">The lock will automatically expire after this duration.</param>
        /// <param name="waitTime">Maximum time to wait for lock acquisition. Defaults to zero (no waiting).</param>
        /// <param name="retryInterval">Time between retry attempts. Defaults to 100ms.</param>
        /// <returns>An <see cref="IDistributedLock"/> if acquired; null otherwise.</returns>
        IDistributedLock? TryAcquire(
            string lockKey,
            TimeSpan expiresIn,
            TimeSpan? waitTime = null,
            TimeSpan? retryInterval = null);

        /// <summary>
        /// Asynchronously tries to acquire a lock, optionally waiting and retrying.
        /// Returns an <see cref="IDistributedLock"/> handle that auto-releases on disposal.
        /// </summary>
        /// <param name="lockKey">The key to lock on.</param>
        /// <param name="expiresIn">The lock will automatically expire after this duration.</param>
        /// <param name="waitTime">Maximum time to wait for lock acquisition. Defaults to zero (no waiting).</param>
        /// <param name="retryInterval">Time between retry attempts. Defaults to 100ms.</param>
        /// <param name="cancellationToken">Cancellation token to cancel the wait.</param>
        /// <returns>An <see cref="IDistributedLock"/> if acquired; null otherwise.</returns>
        Task<IDistributedLock?> TryAcquireAsync(
            string lockKey,
            TimeSpan expiresIn,
            TimeSpan? waitTime = null,
            TimeSpan? retryInterval = null,
            CancellationToken cancellationToken = default);
    }
}
