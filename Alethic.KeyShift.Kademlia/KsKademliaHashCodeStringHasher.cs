using Alethic.Kademlia;

namespace Alethic.KeyShift.Kademlia
{

    /// <summary>
    /// Implements <see cref="IKsKademliaHasher{TKey, TNodeId}"/> for a string key.
    /// </summary>
    public class KsKademliaHashCodeStringHasher : IKsKademliaHasher<string, KNodeId32>
    {

        /// <summary>
        /// Generates a node ID for the given string.
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        KNodeId32 IKsKademliaHasher<string, KNodeId32>.Hash(string key) => new KNodeId32((uint)key.GetHashCode());

    }

}
