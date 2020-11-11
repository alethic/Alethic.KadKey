using System;
using System.Collections.Generic;
using System.Linq;

namespace Alethic.KeyShift
{

    public class KsHostClientProvider<TKey> : IKsHostClientProvider<TKey>
    {

        readonly IEnumerable<IKsHostClientFactory<TKey>> factories;

        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        /// <param name="factories"></param>
        public KsHostClientProvider(IEnumerable<IKsHostClientFactory<TKey>> factories)
        {
            this.factories = factories ?? throw new ArgumentNullException(nameof(factories));
        }

        public IKsHostClient<TKey> Get(Uri uri)
        {
            return factories.Select(i => i.Get(uri)).FirstOrDefault(i => i != null);
        }

    }

}
