using System;
using System.Linq;
using System.Net;
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

        public async Task<KsHostShiftLockResult> GetAsync(TKey key, string token, CancellationToken cancellationToken = default)
        {
            // build GET request optionally with token
            var r = new HttpRequestMessage(HttpMethod.Get, uri.Combine(key.ToString()));
            if (token != null)
                r.Headers.Add("KeyShift-Token", token);

            // send request
            var b = await http.SendAsync(r, cancellationToken);

            // returned a not found error
            if (b.StatusCode == HttpStatusCode.NotFound)
                return new KsHostShiftLockResult(null, null, null);

            // returned a redirect, item must already be forwarded
            if (b.StatusCode == HttpStatusCode.Redirect)
                return new KsHostShiftLockResult(null, null, b.Headers.Location);

            // returned data and a token, item was present and is now frozen
            if (b.StatusCode == HttpStatusCode.OK)
            {
                var t = b.Headers.GetValues("KeyShift-Token").FirstOrDefault();
                if (t == null)
                    throw new KsException("No 'KeyShift-Token' header present in host request.");

                return new KsHostShiftLockResult(t, await b.Content.ReadAsByteArrayAsync(), null);
            }

            throw new KsException("Unable to handle response.");
        }

        public async Task ForwardAsync(TKey key, string token, Uri forward, CancellationToken cancellationToken = default)
        {
            // build DELETE request optionally with token
            var r = new HttpRequestMessage(HttpMethod.Delete, new UriBuilder(uri.Combine(key.ToString())).AppendQuery("forward", forward).Uri);
            if (token != null)
                r.Headers.Add("KeyShift-Token", token);

            var b = await http.SendAsync(r, cancellationToken);
            b.EnsureSuccessStatusCode();
        }

    }

}
