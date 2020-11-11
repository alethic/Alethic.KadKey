using System.Security.Cryptography;
using System.Text;

using Cogito.Kademlia;

namespace Alethic.KeyShift
{

    /// <summary>
    /// Implements <see cref="IKsHash{TKey}"/> for a string key.
    /// </summary>
    public class KsStringHash : IKsHash<string>
    {

        readonly SHA256 sha256 = SHA256.Create();

        /// <summary>
        /// Generates a node ID for the given string.
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public KNodeId256 Hash(string key) => KNodeId<KNodeId256>.Read(sha256.ComputeHash(Encoding.UTF8.GetBytes(key)));

    }

}
