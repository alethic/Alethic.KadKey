using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;

using Cogito.Threading;

using Microsoft.Extensions.Logging;

namespace Alethic.KeyShift.InMemory
{

    /// <summary>
    /// Implements a KeyShift store in memory.
    /// </summary>
    /// <typeparam name="TKey"></typeparam>
    public class KsInMemoryStore<TKey> : IKsStore<TKey>
    {

        readonly ILogger logger;
        readonly ConcurrentDictionary<TKey, AsyncLock> sync = new ConcurrentDictionary<TKey, AsyncLock>();
        readonly ConcurrentDictionary<TKey, KsInMemoryStoreData<TKey>> data = new ConcurrentDictionary<TKey, KsInMemoryStoreData<TKey>>();

        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        /// <param name="logger"></param>
        public KsInMemoryStore(ILogger logger)
        {
            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Opens the entry specified by the key.
        /// </summary>
        /// <param name="key"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public async Task<IKsStoreEntry<TKey>> OpenAsync(TKey key, CancellationToken cancellationToken = default)
        {
            return new KsInMemoryStoreEntry<TKey>(this, key, data.GetOrAdd(key, k => new KsInMemoryStoreData<TKey>()), await sync.GetOrAdd(key, k => new AsyncLock()).LockAsync());
        }

    }

}
