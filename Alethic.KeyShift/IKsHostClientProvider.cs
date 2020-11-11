using System;

namespace Alethic.KeyShift
{

    /// <summary>
    /// Provides instances of <see cref="IKsHostClient{TKey}"/>.
    /// </summary>
    /// <typeparam name="TKey"></typeparam>
    public interface IKsHostClientProvider<TKey>
    {

        /// <summary>
        /// Creates a new <see cref="IKsHostClient{TKey}"/> to the specified remote KeyShift host.
        /// </summary>
        /// <param name="uri"></param>
        /// <returns></returns>
        IKsHostClient<TKey> Get(Uri uri);

    }

}
