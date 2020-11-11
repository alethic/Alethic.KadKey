using System;
using System.Net.Http;

namespace Alethic.KeyShift
{

    /// <summary>
    /// Generates instances of the <see cref="KsHostHttpClient{TKey}"/>.
    /// </summary>
    /// <typeparam name="TKey"></typeparam>
    public class KsHostHttpClientFactory<TKey> : IKsHostClientFactory<TKey>
    {

        readonly HttpClient http;

        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        /// <param name="http"></param>
        public KsHostHttpClientFactory(HttpClient http)
        {
            this.http = http ?? throw new ArgumentNullException(nameof(http));
        }

        public IKsHostClient<TKey> Get(Uri uri)
        {
            if (uri.Scheme == Uri.UriSchemeHttp ||
                uri.Scheme == Uri.UriSchemeHttps)
                return new KsHostHttpClient<TKey>(http, uri);

            return null;
        }

    }

}
