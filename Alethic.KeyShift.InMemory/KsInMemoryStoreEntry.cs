using System;
using System.Threading;
using System.Threading.Tasks;

using Cogito.Threading;

namespace Alethic.KeyShift.InMemory
{

    class KsInMemoryStoreEntry<TKey> : IKsStoreEntry<TKey>
    {

        readonly KsInMemoryStore<TKey> store;
        readonly TKey key;
        readonly KsInMemoryStoreData<TKey> data;
        readonly AsyncLock.AsyncLockHandle handle;

        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        /// <param name="key"></param>
        /// <param name="data"></param>
        /// <param name="handle"></param>
        public KsInMemoryStoreEntry(KsInMemoryStore<TKey> store, TKey key, KsInMemoryStoreData<TKey> data, AsyncLock.AsyncLockHandle handle)
        {
            this.store = store ?? throw new ArgumentNullException(nameof(store));
            this.key = key;
            this.data = data ?? throw new ArgumentNullException(nameof(data));
            this.handle = handle;
        }

        /// <summary>
        /// Gets the associated key represented by this entry.
        /// </summary>
        public TKey Key => key;

        /// <summary>
        /// Gets the current data of the entry. If the entry is frozen the call will block.
        /// </summary>
        /// <param name="token"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public async Task<KsStoreGetResult> GetAsync(string token, CancellationToken cancellationToken = default)
        {
            if (data.FreezeLockToken != null && data.FreezeLockToken != token)
                await data.FreezeLockTimer;

            if (data.Forward != null)
                return new KsStoreGetResult(null, data.Forward);

            return new KsStoreGetResult(data.Data, null);
        }

        /// <summary>
        /// Saves new data associated with the entry.
        /// </summary>
        /// <param name="value"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public Task SetAsync(ReadOnlySpan<byte> value, CancellationToken cancellationToken = default)
        {
            return SetAsyncImpl(value.ToArray(), cancellationToken);
        }

        /// <summary>
        /// Saves new data associated with the entry.
        /// </summary>
        /// <param name="value"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        async Task SetAsyncImpl(byte[] value, CancellationToken cancellationToken = default)
        {
            if (data.FreezeLockToken != null)
                await data.FreezeLockTimer;

            // reset state to hosting data
            data.FreezeLockToken = null;
            data.FreezeLockCancel?.Cancel();
            data.FreezeLockCancel = null;
            data.FreezeLockTimer = null;
            data.Forward = null;
            data.Data = value;
        }

        /// <summary>
        /// Runs a task that times out when the freeze expires.
        /// </summary>
        /// <param name="store"></param>
        /// <param name="key"></param>
        /// <param name="timeout"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        static async Task FreezeTimeoutTask(KsInMemoryStore<TKey> store, TKey key, TimeSpan timeout, CancellationToken cancellationToken)
        {
            // wait the appropriate timeout
            await Task.Delay(timeout, cancellationToken);

            // erase the freeze data
            await using var entry = (KsInMemoryStoreEntry<TKey>)await store.OpenAsync(key, cancellationToken);
            entry.data.FreezeLockTimer = null;
            entry.data.FreezeLockCancel = null;
            entry.data.FreezeLockToken = null;
        }

        /// <summary>
        /// Marks the entry as frozen for the specified time. Returns a token that can be used to overide the freeze
        /// and complete the shift.
        /// </summary>
        /// <param name="timeout"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public async Task<KsStoreFreezeResult> FreezeAsync(string token, TimeSpan timeout, CancellationToken cancellationToken = default)
        {
            if (data.FreezeLockToken != null && data.FreezeLockToken != token)
                await data.FreezeLockTimer;

            if (data.Forward != null)
                return new KsStoreFreezeResult(null, data.Forward);

            // remove existing freeze
            data.FreezeLockToken = null;
            data.FreezeLockCancel?.Cancel();
            data.FreezeLockCancel = null;
            data.FreezeLockTimer = null;

            // set freeze state and begin timer
            data.Forward = null;
            data.FreezeLockToken = Guid.NewGuid().ToString();
            data.FreezeLockCancel = new CancellationTokenSource();
            data.FreezeLockTimer = FreezeTimeoutTask(store, key, timeout, data.FreezeLockCancel.Token);

            return new KsStoreFreezeResult(data.FreezeLockToken, null);
        }

        /// <summary>
        /// Removes the entry, adding a forward endpoint.
        /// </summary>
        /// <param name="token"></param>
        /// <param name="forward"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public async Task ForwardAsync(string token, Uri forward, CancellationToken cancellationToken = default)
        {
            if (data.FreezeLockToken != null && data.FreezeLockToken != token)
                await data.FreezeLockTimer;

            data.Data = null;
            data.Forward = forward;
            data.FreezeLockToken = null;
            data.FreezeLockCancel?.Cancel();
            data.FreezeLockCancel = null;
            data.FreezeLockTimer = null;
        }

        /// <summary>
        /// Gets the token for the current owner. Used as part of the shifting process.
        /// </summary>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public Task<string> GetOwnerTokenAsync(CancellationToken cancellationToken = default)
        {
            return Task.FromResult(data.OwnerLockToken);
        }

        /// <summary>
        /// Sets the token for the current owner. Used as part of the shifting process.
        /// </summary>
        /// <param name="token"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public Task SetOwnerTokenAsync(string token, CancellationToken cancellationToken = default)
        {
            data.OwnerLockToken = token;
            return Task.CompletedTask;
        }

        /// <summary>
        /// Disposes of the instance and releases the entry.
        /// </summary>
        /// <returns></returns>
        public ValueTask DisposeAsync()
        {
            handle.Dispose();
            return new ValueTask(Task.CompletedTask);
        }
    }

}
