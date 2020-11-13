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

        public async Task<KsHostShiftLockResult?> ShiftLockAsync(TKey key, string token, CancellationToken cancellationToken = default)
        {
            // build GET request optionally with token
            var r = new HttpRequestMessage(HttpMethod.Get, uri.Combine(key.ToString()));
            if (token != null)
                r.Headers.Add("KeyShift-Token", token);

            // send request
            var b = await http.SendAsync(r, cancellationToken);

            // returned a not found error
            if (b.StatusCode == HttpStatusCode.NotFound)
                return null;

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

        public async Task<KsHostShiftResult?> ShiftAsync(TKey key, string token, Uri forwardUri, CancellationToken cancellationToken = default)
        {
            // build DELETE request optionally with token
            var r = new HttpRequestMessage(HttpMethod.Delete, uri.Combine(key.ToString()));
            r.Headers.Add("KeyShift-Token", token);
            r.Headers.Add("KeyShift-ForwardUri", forwardUri.ToString());

            var b = await http.SendAsync(r, cancellationToken);

            if (b.StatusCode == HttpStatusCode.NotFound)
                return null;

            // returned a redirect, item must already be forwarded
            if (b.StatusCode == HttpStatusCode.Redirect)
                return new KsHostShiftResult(b.Headers.Location);

            if (b.StatusCode == HttpStatusCode.OK)
                return new KsHostShiftResult(null);

            throw new KsException("Unable to handle response.");
        }

    }

}
