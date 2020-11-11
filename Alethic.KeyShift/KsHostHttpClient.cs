using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

using Cogito;

namespace Alethic.KeyShift
{

    public class KsHostHttpClient<TKey> : IKsHostClient<TKey>
    {

        readonly HttpClient http;
        readonly Uri uri;

        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        /// <param name="http"></param>
        public KsHostHttpClient(HttpClient http, Uri uri)
        {
            this.http = http ?? throw new ArgumentNullException(nameof(http));
            this.uri = uri ?? throw new ArgumentNullException(nameof(uri));
        }

        public async Task<byte[]> Select(TKey key, Guid? token, CancellationToken cancellationToken = default)
        {
            return await http.GetByteArrayAsync(new UriBuilder(uri.Combine(key.ToString())).AppendQuery("token", token).Uri);
        }

        public async Task<Guid> Freeze(TKey key, Guid? token, CancellationToken cancellationToken = default)
        {
            return Guid.Parse(await http.GetStringAsync(new UriBuilder(uri.Combine(key.ToString(), "freeze")).AppendQuery("token", token).Uri));
        }

        public async Task Remove(TKey key, Guid? token, Uri forward, CancellationToken cancellationToken = default)
        {
            await http.DeleteAsync(new UriBuilder(uri.Combine(key.ToString())).AppendQuery("token", token).AppendQuery("forward", forward.ToString()).Uri);
        }

    }

}
