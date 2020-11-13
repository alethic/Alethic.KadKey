using System;
using System.Threading;
using System.Threading.Tasks;

namespace Alethic.KeyShift
{

    /// <summary>
    /// A <see cref="IKsHost"/> represents the main operations of the KeyShift host.
    /// </summary>
    public interface IKsHost<TKey>
    {

        /// <summary>
        /// Gets the current data for the host as part of the shifting process, establishes a timeout for the shift to be completed.
        /// </summary>
        /// <param name="key"></param>
        /// <param name="token"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        Task<KsHostShiftLockResult> ShiftLockAsync(TKey key, string token, CancellationToken cancellationToken);

        /// <summary>
        /// Finishes the shifting process by removing host data and establishing a forward.
        /// </summary>
        /// <param name="key"></param>
        /// <param name="token"></param>
        /// <param name="forward"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        Task ShiftAsync(TKey key, string token, Uri forward, CancellationToken cancellationToken);

        /// <summary>
        /// Gets the current value of the specified key. Ownership of the key is transfered to this host.
        /// </summary>
        /// <param name="key"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        Task<byte[]> GetAsync(TKey key, CancellationToken cancellationToken);

        /// <summary>
        /// Updates the current value of the specified key. Ownership of the key is transfered to this host.
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        Task SetAsync(TKey key, byte[] value, CancellationToken cancellationToken);

    }

}
