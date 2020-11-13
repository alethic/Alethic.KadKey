using System.Threading;
using System.Threading.Tasks;

namespace Alethic.KeyShift
{

    /// <summary>
    /// Represents the hashtable KeyShift will use to distribute owner records.
    /// </summary>
    /// <typeparam name="TKey"></typeparam>
    public interface IKsHashTable<TKey>
    {

        /// <summary>
        /// Gets the particular value from the hashtable.
        /// </summary>
        /// <param name="key"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        Task<KsHashTableValue?> GetAsync(TKey key, CancellationToken cancellationToken = default);

        /// <summary>
        /// Adds the particular value in the hashtable.
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        Task<KsHashTableValue?> AddAsync(TKey key, KsHashTableValue? value, CancellationToken cancellationToken = default);

        /// <summary>
        /// Removes the particular value in the hashtable.
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        Task RemoveAsync(TKey key, CancellationToken cancellationToken = default);

    }

}
