using System.Threading;
using System.Threading.Tasks;

namespace Alethic.KeyShift
{

    /// <summary>
    /// Represents an interface to the backend store that holds the key data.
    /// </summary>
    /// <typeparam name="TKey"></typeparam>
    public interface IKsStore<TKey>
    {

        /// <summary>
        /// Opens access to the entry with the specified key. Other access should be blocked until this handle is released.
        /// </summary>
        /// <param name="key"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        Task<IKsStoreEntry<TKey>> OpenAsync(TKey key, CancellationToken cancellationToken = default);

    }

}
