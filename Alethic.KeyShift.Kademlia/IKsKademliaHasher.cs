namespace Alethic.KeyShift.Kademlia
{

    /// <summary>
    /// Provides a hash algorithm for generating a node ID from a key.
    /// </summary>
    /// <typeparam name="TKey"></typeparam>
    /// <typeparam name="TNodeId"></typeparam>
    public interface IKsKademliaHasher<TKey, TNodeId>
        where TNodeId : unmanaged
    {

        /// <summary>
        /// Generates a node ID for the given key.
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        TNodeId Hash(TKey key);

    }

}
