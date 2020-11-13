using System;
using System.Threading;
using System.Threading.Tasks;

namespace Alethic.KeyShift
{

    /// <summary>
    /// Represents a handle to an item in the store.
    /// </summary>
    /// <typeparam name="TKey"></typeparam>
    public interface IKsStoreEntry<TKey> : IAsyncDisposable
    {

        /// <summary>
        /// Gets the key represented by this entry.
        /// </summary>
        TKey Key { get; }

        /// <summary>
        /// Gets the current data of the entry. If the entry is frozen, <c>null</c> will be returned, unless the token
        /// is the appropriate freeze token.
        /// </summary>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        Task<KsStoreGetResult> GetAsync(string token, CancellationToken cancellationToken = default);

        /// <summary>
        /// Saves new data associated with the entry.
        /// </summary>
        /// <param name="value"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        Task SetAsync(ReadOnlySpan<byte> value, CancellationToken cancellationToken = default);

        /// <summary>
        /// Marks the entry as frozen for the specified time. Returns a token that can be used to overide the freeze
        /// and complete the shift.
        /// </summary>
        /// <param name="token"></param>
        /// <param name="timeout"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        Task<KsStoreFreezeResult> FreezeAsync(string token, TimeSpan timeout, CancellationToken cancellationToken = default);

        /// <summary>
        /// Marks the entry as forwarded to another owner. Future attempts to get the data will return an error.
        /// </summary>
        /// <param name="token"></param>
        /// <param name="forward"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        Task ForwardAsync(string token, Uri forward, CancellationToken cancellationToken = default);

        /// <summary>
        /// Gets the token for the current owner. Used as part of the shifting process.
        /// </summary>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        Task<string> GetOwnerTokenAsync(CancellationToken cancellationToken = default);

        /// <summary>
        /// Sets the token for the current owner. Used as part of the shifting process.
        /// </summary>
        /// <param name="token"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        Task SetOwnerTokenAsync(string token, CancellationToken cancellationToken = default);

    }

}
