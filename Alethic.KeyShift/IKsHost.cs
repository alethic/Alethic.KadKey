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
        /// Gets the current value of the specified key. Ownership of the key is transfered to this host.
        /// </summary>
        /// <param name="key"></param>
        /// <param name="token"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        Task<byte[]> Select(TKey key, Guid? token, CancellationToken cancellationToken);

        /// <summary>
        /// Updates the current value of the specified key. Ownership of the key is transfered to this host.
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        Task Update(TKey key, byte[] value, CancellationToken cancellationToken);

        /// <summary>
        /// Initiates a limited time freeze on the key, causing readers and writers to block. Used to initiate a
        /// transfer. A token that allows the completion of the transfer is returned.
        /// </summary>
        /// <param name="key"></param>
        /// <param name="token"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        Task<Guid> Freeze(TKey key, Guid? token, CancellationToken cancellationToken);

        /// <summary>
        /// Removes the key from the host. Invoked to finalize a transfer. 
        /// </summary>
        /// <param name="key"></param>
        /// <param name="token"></param>
        /// <param name="forward"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        Task Remove(TKey key, Guid token, Uri forward, CancellationToken cancellationToken);

    }

}
