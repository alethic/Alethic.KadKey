using System.Security.Cryptography;
using System.Text;

using Alethic.Kademlia;

namespace Alethic.KeyShift.Kademlia
{

    /// <summary>
    /// Implements <see cref="IKsKademliaHasher{TKey, TNodeId}"/> for a string key.
    /// </summary>
    public class KsKademliaSha1StringHasher : IKsKademliaHasher<string, KNodeId160>, IKsKademliaHasher<string, KNodeId128>, IKsKademliaHasher<string, KNodeId64>
    {

        readonly SHA1 sha1 = SHA1.Create();

        /// <summary>
        /// Generates a node ID for the given string.
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        KNodeId160 IKsKademliaHasher<string, KNodeId160>.Hash(string key) => KNodeId<KNodeId160>.Read(sha1.ComputeHash(Encoding.UTF8.GetBytes(key)));

        /// <summary>
        /// Generates a node ID for the given string.
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        KNodeId128 IKsKademliaHasher<string, KNodeId128>.Hash(string key) => KNodeId<KNodeId128>.Read(sha1.ComputeHash(Encoding.UTF8.GetBytes(key)));

        /// <summary>
        /// Generates a node ID for the given string.
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        KNodeId64 IKsKademliaHasher<string, KNodeId64>.Hash(string key) => KNodeId<KNodeId64>.Read(sha1.ComputeHash(Encoding.UTF8.GetBytes(key)));

    }

}
