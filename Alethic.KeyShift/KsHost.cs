using System;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

using Newtonsoft.Json;

namespace Alethic.KeyShift
{

    /// <summary>
    /// Manages known data. Provides operations to Get, Set. These are available to the clients. Other servers can invoke Lock, Get and Transfer.
    /// </summary>
    public class KsHost<TKey> : IKsHost<TKey>
    {

        readonly IOptions<KsHostOptions> options;
        readonly IKsStore<TKey> store;
        readonly IKsHostClientProvider<TKey> clients;
        readonly IKsHashTable<TKey> hashtable;
        readonly ILogger logger;

        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        /// <param name="options"></param>
        /// <param name="store"></param>
        /// <param name="hashtable"></param>
        /// <param name="clients"></param>
        /// <param name="logger"></param>
        public KsHost(IOptions<KsHostOptions> options, IKsStore<TKey> store, IKsHashTable<TKey> hashtable, IKsHostClientProvider<TKey> clients, ILogger logger)
        {
            this.options = options ?? throw new ArgumentNullException(nameof(options));
            this.store = store ?? throw new ArgumentNullException(nameof(store));
            this.hashtable = hashtable ?? throw new ArgumentNullException(nameof(hashtable));
            this.clients = clients ?? throw new ArgumentNullException(nameof(clients));
            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Serializes the entry information.
        /// </summary>
        /// <param name="entry"></param>
        /// <returns></returns>
        byte[] Serialize(KsHashTableEntry entry)
        {
            return Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(entry));
        }

        /// <summary>
        /// Serializes the entry information.
        /// </summary>
        /// <param name="entry"></param>
        /// <returns></returns>
        KsHashTableEntry Deserialize(byte[] data)
        {
            return JsonConvert.DeserializeObject<KsHashTableEntry>(Encoding.UTF8.GetString(data));
        }

        /// <summary>
        /// Transfers the data with the key and entry to the local store.
        /// </summary>
        /// <param name="entry"></param>
        /// <param name="value"></param>
        /// <param name="dht"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        async Task TransferAsync(IKsStoreEntry<TKey> entry, KsHashTableValue value, KsHashTableEntry dht, CancellationToken cancellationToken)
        {
            foreach (var uri in dht.Endpoints)
            {
                // we can't pull the value from ourselves, move to secondaries
                if (uri == options.Value.Uri)
                    continue;

                var client = clients.Get(uri);
                if (client == null)
                    throw new KsException($"Could not obtain client for remote host: '{uri}'");

                // existing token if we're resuming an operation after failure
                var t = await entry.GetOwnerTokenAsync(cancellationToken);

                // trace down current owner
                var v = await client.GetAsync(entry.Key, t, cancellationToken);
                while (v.ForwardUri != null)
                {
                    client = clients.Get(v.ForwardUri);
                    v = await client.GetAsync(entry.Key, t, cancellationToken);
                }

                if (v.Data == null)
                    throw new KsException("Data not retrieved.");
                if (v.Token == null)
                    throw new KsException("Data retrieved, but not token.");

                // update local entry with latest information
                await entry.SetOwnerTokenAsync(t = v.Token);
                await entry.SetAsync(v.Data);

                // update DHT with new version
                // TODO establish secondaries
                await hashtable.AddAsync(entry.Key, new KsHashTableValue(Serialize(new KsHashTableEntry(new[] { options.Value.Uri })), value.Version + 1, TimeSpan.FromMinutes(60)));

                // signal remote node to remove value and forward
                await client.ForwardAsync(entry.Key, t, options.Value.Uri, cancellationToken);

                // we succeeded, exit loop
                break;
            }

            // zero out lock token if we successfully complete process
            await entry.SetOwnerTokenAsync(null);
        }

        /// <summary>
        /// Executes the given function after ensuring ownership of the key.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key"></param>
        /// <param name="func"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        async Task<T> Access<T>(TKey key, Func<IKsStoreEntry<TKey>, CancellationToken, Task<T>> func, bool shift, CancellationToken cancellationToken)
        {
            await using var entry = await store.OpenAsync(key, cancellationToken);

            var v = await hashtable.GetAsync(key, cancellationToken);

            // value is unknown to the system
            if (v == null)
            {
                // initial publish of DHT record
                v = new KsHashTableValue(Serialize(new KsHashTableEntry(new[] { options.Value.Uri })), 1, TimeSpan.FromDays(1));
                await hashtable.AddAsync(key, v, cancellationToken);
                return await func(entry, cancellationToken);
            }

            // try to transfer object until transfer succeeds
            if (shift)
            {
                var e = Deserialize(v.Value.Data);
                while (e.Endpoints.Length == 0 || e.Endpoints[0] != options.Value.Uri)
                {
                    await TransferAsync(entry, v.Value, e, cancellationToken);
                    v = await hashtable.GetAsync(key, cancellationToken);
                    e = Deserialize(v.Value.Data);
                }
            }

            return await func(entry, cancellationToken);
        }

        /// <summary>
        /// Executes the given function after ensuring ownership of the key.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key"></param>
        /// <param name="func"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        Task Access(TKey key, Func<IKsStoreEntry<TKey>, CancellationToken, Task> func, bool shift, CancellationToken cancellationToken)
        {
            return Access(key, (k, c) => { func(k, c); return Task.FromResult(true); }, shift, cancellationToken);
        }

        /// <summary>
        /// Gets the value of the specified key.
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public Task<KsHostShiftLockResult> ShiftLockAsync(TKey key, string token, CancellationToken cancellationToken)
        {
            return Access(key, (e, c) => ShiftLockAsyncImpl(e, token, c), false, cancellationToken);
        }

        async Task<KsHostShiftLockResult> ShiftLockAsyncImpl(IKsStoreEntry<TKey> entry, string token, CancellationToken cancellationToken)
        {
            var t = await entry.FreezeAsync(token, TimeSpan.FromSeconds(5), cancellationToken);
            var d = await entry.GetAsync(t, cancellationToken);
            return new KsHostShiftLockResult(t, d.Data, d.ForwardUri);
        }

        /// <summary>
        /// Removes the value specified by the key and token.
        /// </summary>
        /// <param name="key"></param>
        /// <param name="token"></param>
        /// <param name="forwardUri"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public Task ShiftAsync(TKey key, string token, Uri forwardUri, CancellationToken cancellationToken)
        {
            return Access(key, async (e, c) => await ShiftAsyncImpl(e, token, forwardUri, c), false, cancellationToken);
        }

        async Task ShiftAsyncImpl(IKsStoreEntry<TKey> entry, string token, Uri forwardUri, CancellationToken cancellationToken)
        {
            await hashtable.RemoveAsync(entry.Key, CancellationToken.None);
            await entry.ForwardAsync(token, forwardUri, CancellationToken.None);
        }

        /// <summary>
        /// Updates the value of the specified key.
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public Task<byte[]> GetAsync(TKey key, CancellationToken cancellationToken)
        {
            return Access(key, (e, c) => GetAsyncImpl(e, c), true, cancellationToken);
        }

        async Task<byte[]> GetAsyncImpl(IKsStoreEntry<TKey> entry, CancellationToken cancellationToken)
        {
            var r = await entry.GetAsync(null, cancellationToken);
            return r.Data;
        }

        /// <summary>
        /// Updates the value of the specified key.
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public Task SetAsync(TKey key, byte[] value, CancellationToken cancellationToken)
        {
            return Access(key, (e, c) => e.SetAsync(value, c), true, cancellationToken);
        }

    }

}
