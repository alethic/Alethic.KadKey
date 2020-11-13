using System;
using System.Threading;
using System.Threading.Tasks;

namespace Alethic.KeyShift.InMemory
{

    /// <summary>
    /// Represents the data about a known key.
    /// </summary>
    /// <typeparam name="TKey"></typeparam>
    class KsInMemoryStoreData<TKey>
    {

        /// <summary>
        /// Gets or sets the data stored by the key.
        /// </summary>
        public byte[] Data { get; set; }

        /// <summary>
        /// Gets or sets the timer used to cancel any outstanding freezes.
        /// </summary>
        public Task FreezeLockTimer { get; set; }

        /// <summary>
        /// Gets or sets the cancellation token source that is used to cancel the forward after a timeout.
        /// </summary>
        public CancellationTokenSource FreezeLockCancel { get; set; }

        /// <summary>
        /// Gets or sets the token associated with the current freeze.
        /// </summary>
        public string FreezeLockToken { get; set; }

        /// <summary>
        /// If present, indicates the data is available at the remote host.
        /// </summary>
        public Uri Forward { get; set; }

        /// <summary>
        /// During an outstanding shift, stores the lock token of the current owner.
        /// </summary>
        public string OwnerLockToken { get; set; }
    }

}