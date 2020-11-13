using System;
using System.Threading;
using System.Threading.Tasks;

using Alethic.Kademlia;

namespace Alethic.KeyShift.Kademlia
{

    /// <summary>
    /// Provides a <see cref="IKsHashTable{TKey}"/> backing using the Alethic.Kademlia package.
    /// </summary>
    /// <typeparam name="TKey"></typeparam>
    /// <typeparam name="TNodeId"></typeparam>
    public class KsKademliaHashTable<TKey, TNodeId> : IKsHashTable<TKey>
        where TNodeId : unmanaged
    {

        readonly IKsKademliaHasher<TKey, TNodeId> hasher;
        readonly IKValueAccessor<TNodeId> values;
        readonly IKPublisher<TNodeId> publisher;

        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        /// <param name="hasher"></param>
        /// <param name="values"></param>
        /// <param name="publisher"></param>
        public KsKademliaHashTable(IKsKademliaHasher<TKey, TNodeId> hasher, IKValueAccessor<TNodeId> values, IKPublisher<TNodeId> publisher)
        {
            this.hasher = hasher ?? throw new ArgumentNullException(nameof(hasher));
            this.values = values ?? throw new ArgumentNullException(nameof(values));
            this.publisher = publisher ?? throw new ArgumentNullException(nameof(publisher));
        }

        /// <summary>
        /// Gets the value for the key from the hashtable.
        /// </summary>
        /// <param name="key"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public async Task<KsHashTableValue?> GetAsync(TKey key, CancellationToken cancellationToken = default)
        {
            var h = hasher.Hash(key);
            var v = await values.GetAsync(h, cancellationToken);
            return v != null ? new KsHashTableValue(v.Value.Data, v.Value.Version, v.Value.Expiration - DateTime.Now) : (KsHashTableValue?)null;
        }

        /// <summary>
        /// Gets the value for the key from the hashtable.
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public async Task<KsHashTableValue?> AddAsync(TKey key, KsHashTableValue? value, CancellationToken cancellationToken = default)
        {
            var h = hasher.Hash(key);

            // obtain existing value and remove
            var v = await publisher.GetAsync(h, cancellationToken);
            if (v != null)
                await publisher.RemoveAsync(h, cancellationToken);

            // if we're publishing a value, add it
            if (value != null)
            {
                v = new KValueInfo(value.Value.Data, value.Value.Version, DateTime.Now.Add(value.Value.TimeToLive));
                await publisher.AddAsync(hasher.Hash(key), v.Value, cancellationToken);
                return v != null ? new KsHashTableValue(v.Value.Data, v.Value.Version, v.Value.Expiration - DateTime.Now) : (KsHashTableValue?)null;
            }

            return null;
        }

        /// <summary>
        /// Removes the specified key from the hashtable.
        /// </summary>
        /// <param name="key"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public async Task RemoveAsync(TKey key, CancellationToken cancellationToken = default)
        {
            await publisher.RemoveAsync(hasher.Hash(key), cancellationToken);
        }

    }

}
