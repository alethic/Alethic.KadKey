
using Cogito.Kademlia;

namespace Alethic.KeyShift
{

    /// <summary>
    /// Provides a hash algorithm for generating a node ID from a key.
    /// </summary>
    /// <typeparam name="TKey"></typeparam>
    public interface IKsHash<TKey>
    {

        /// <summary>
        /// Generates a node ID for the given key.
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        KNodeId256 Hash(TKey key);

    }

}
