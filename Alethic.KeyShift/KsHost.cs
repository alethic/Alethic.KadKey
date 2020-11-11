using System;
using System.Collections.Concurrent;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using Cogito.Kademlia;
using Cogito.Threading;

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

        /// <summary>
        /// Entry of the service locator within the DHT.
        /// </summary>
        class DhtEntry
        {

            /// <summary>
            /// Services URIs that are the current primary and secondaries of the key data.
            /// </summary>
            public Uri[] Uri { get; set; }

        }

        /// <summary>
        /// Entry of the data within the store.
        /// </summary>
        class KeyEntry
        {

            /// <summary>
            /// Real data of the key.
            /// </summary>
            public byte[] Data { get; set; }

            /// <summary>
            /// Lock established during freeze.
            /// </summary>
            public Task Lock { get; set; }

            /// <summary>
            /// Cancels any outstanding lock.
            /// </summary>
            public CancellationTokenSource LockCancel { get; internal set; }

            /// <summary>
            /// Associated lock token.
            /// </summary>
            public Guid? LockToken { get; set; }

            /// <summary>
            /// Associated lock token of the peer.
            /// </summary>
            public Guid? PeerLockToken { get; set; }

            /// <summary>
            /// New destination of the key.
            /// </summary>
            public Uri To { get; internal set; }

        }

        readonly IOptions<KsHostOptions> options;
        readonly IKsHash<TKey> hash;
        readonly IKsHostClientProvider<TKey> clients;
        readonly IKPublisher<KNodeId256> publisher;
        readonly IKValueAccessor<KNodeId256> values;
        readonly HttpClient http;
        readonly ILogger logger;

        readonly ConcurrentDictionary<TKey, AsyncLock> syncs = new ConcurrentDictionary<TKey, AsyncLock>();
        readonly ConcurrentDictionary<TKey, KeyEntry> data = new ConcurrentDictionary<TKey, KeyEntry>();

        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        /// <param name="options"></param>
        /// <param name="hash"></param>
        /// <param name="clients"></param>
        /// <param name="values"></param>
        /// <param name="publisher"></param>
        /// <param name="http"></param>
        /// <param name="logger"></param>
        public KsHost(IOptions<KsHostOptions> options, IKsHash<TKey> hash, IKsHostClientProvider<TKey> clients, IKPublisher<KNodeId256> publisher, IKValueAccessor<KNodeId256> values, HttpClient http, ILogger logger)
        {
            this.options = options ?? throw new ArgumentNullException(nameof(options));
            this.hash = hash ?? throw new ArgumentNullException(nameof(hash));
            this.clients = clients ?? throw new ArgumentNullException(nameof(clients));
            this.publisher = publisher ?? throw new ArgumentNullException(nameof(publisher));
            this.values = values ?? throw new ArgumentNullException(nameof(values));
            this.http = http ?? throw new ArgumentNullException(nameof(http));
            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Generates a node ID for the specified key.
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        KNodeId256 Hash(TKey key) => hash.Hash(key);

        /// <summary>
        /// Serializes the entry information.
        /// </summary>
        /// <param name="entry"></param>
        /// <returns></returns>
        byte[] Serialize(DhtEntry entry)
        {
            return Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(entry));
        }

        /// <summary>
        /// Serializes the entry information.
        /// </summary>
        /// <param name="entry"></param>
        /// <returns></returns>
        DhtEntry Deserialize(byte[] data)
        {
            return JsonConvert.DeserializeObject<DhtEntry>(Encoding.UTF8.GetString(data));
        }

        /// <summary>
        /// Gets the synchronization lock for the specified key.
        /// </summary>
        /// <param name="key"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        Task<IDisposable> Sync(TKey key, CancellationToken cancellationToken)
        {
            return syncs.GetOrAdd(key, k => new AsyncLock()).LockAsync(cancellationToken);
        }

        /// <summary>
        /// Transfers the data with the key and entry to the local store.
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <param name="dht"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        async Task TransferAsync(TKey key, KValueInfo value, DhtEntry dht, CancellationToken cancellationToken)
        {
            // check if we already own the entry
            var val = data.GetOrAdd(key, _ => new KeyEntry());
            if (val.Data != null)
                return;

            foreach (var uri in dht.Uri)
            {
                // we can't pull the value from ourselves, move to secondaries
                if (uri == options.Value.Uri)
                    continue;

                var client = clients.Get(uri);
                if (client == null)
                    throw new KsException($"Could not obtain client for remote host: '{uri}'");

                // attempt to freeze remote entry and acquire token
                val.PeerLockToken = await client.Freeze(key, val.PeerLockToken, cancellationToken);

                // retrieve existing value
                val.Data = await client.Select(key, val.PeerLockToken, cancellationToken);
                val.Lock = null;
                val.LockToken = null;
                val.To = null;

                // update DHT with new version
                // TODO establish secondaries
                dht.Uri = new[] { options.Value.Uri };
                await publisher.AddAsync(Hash(key), new KValueInfo(Serialize(dht), value.Version + 1, DateTime.UtcNow.AddMinutes(60)));

                // signal remote node to remove value and forward
                await client.Remove(key, val.PeerLockToken, options.Value.Uri);
                val.PeerLockToken = null;

                // we succeeded, exit loop
                break;
            }
        }

        /// <summary>
        /// Executes the given function after ensuring ownership of the key.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key"></param>
        /// <param name="token"></param>
        /// <param name="func"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        async Task<T> Access<T>(TKey key, Guid? token, Func<TKey, KeyEntry, CancellationToken, Task<T>> func, bool move, CancellationToken cancellationToken)
        {
            using (await Sync(key, cancellationToken))
            {
                var l = data.GetOrAdd(key, k => new KeyEntry());
                var h = Hash(key);
                var v = await values.GetAsync(h, cancellationToken);

                // value is unknown to the system
                if (v == null)
                {
                    // initial publish of DHT record
                    await publisher.AddAsync(h, new KValueInfo(Serialize(new DhtEntry() { Uri = new Uri[] { options.Value.Uri } }), 1, DateTime.UtcNow.AddDays(1)), cancellationToken);
                    return await func(key, l, cancellationToken);
                }

                // try to transfer object until transfer succeeds
                if (move)
                {
                    var e = Deserialize(v.Value.Data);
                    while (e.Uri.Length == 0 || e.Uri[0] != options.Value.Uri)
                    {
                        await TransferAsync(key, v.Value, e, cancellationToken);
                        v = await values.GetAsync(h, cancellationToken);
                        e = Deserialize(v.Value.Data);
                    }
                }

                // if we're currently locked, wait until lock is released
                if (l != null && l.Lock != null && l.LockToken != token)
                    await l.Lock;

                return await func(key, l, cancellationToken);
            }
        }

        /// <summary>
        /// Gets the value of the specified key.
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public Task<byte[]> Select(TKey key, Guid? token, CancellationToken cancellationToken)
        {
            return Access(key, token, SelectInternal, true, cancellationToken);
        }

        Task<byte[]> SelectInternal(TKey key, KeyEntry entry, CancellationToken cancellationToken)
        {
            return Task.FromResult(entry.Data);
        }

        /// <summary>
        /// Updates the value of the specified key.
        /// </summary>
        /// <param name="key"></param>
        /// <param name="buffer"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public Task Update(TKey key, byte[] buffer, CancellationToken cancellationToken)
        {
            return Access(key, null, (k, e, c) => UpdateInternal(k, e, buffer, c), true, cancellationToken);
        }

        Task<bool> UpdateInternal(TKey key, KeyEntry entry, byte[] buffer, CancellationToken cancellationToken)
        {
            entry.Data = buffer;
            return Task.FromResult(true);
        }

        /// <summary>
        /// Places a temporary lock on modification of the value with the specified key and returns a token.
        /// </summary>
        /// <param name="key"></param>
        /// <param name="token"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public Task<Guid> Freeze(TKey key, Guid? token, CancellationToken cancellationToken)
        {
            return Access(key, token, (k, e, c) => FreezeInternal(k, e, token, c), false, cancellationToken);
        }

        Task<Guid> FreezeInternal(TKey key, KeyEntry entry, Guid? token, CancellationToken cancellationToken)
        {
            entry.Lock = Task.Run(async () => { await Task.Delay(TimeSpan.FromSeconds(5)); using (await Sync(key, CancellationToken.None)) { entry.Lock = null; entry.LockCancel = null; entry.LockToken = null; } return true; });
            entry.LockCancel = new CancellationTokenSource();
            entry.LockToken = token ?? Guid.NewGuid();
            return Task.FromResult(entry.LockToken.Value);
        }

        /// <summary>
        /// Removes the value specified by the key and token.
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public Task Remove(TKey key, Guid token, Uri forward, CancellationToken cancellationToken)
        {
            return Access(key, token, (k, e, c) => RemoveInternal(k, e, forward, c), false, cancellationToken);
        }

        async Task<bool> RemoveInternal(TKey key, KeyEntry entry, Uri forward, CancellationToken cancellationToken)
        {
            await publisher.RemoveAsync(Hash(key), cancellationToken);
            entry.Data = null;
            entry.Lock = null;
            entry.LockCancel?.Cancel();
            entry.LockCancel = null;
            entry.LockToken = null;
            entry.To = forward;
            return true;
        }

    }

}
