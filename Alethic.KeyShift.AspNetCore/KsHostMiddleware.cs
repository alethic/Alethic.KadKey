using System;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;

namespace Alethic.KeyShift.AspNetCore
{

    /// <summary>
    /// Implements the KeyShift backend hosting APIs.
    /// </summary>
    public class KsHostMiddleware : IMiddleware
    {

        readonly IKsHost<string> host;

        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        /// <param name="host"></param>
        public KsHostMiddleware(IKsHost<string> host)
        {
            this.host = host ?? throw new ArgumentNullException(nameof(host));
        }

        public Task InvokeAsync(HttpContext context, RequestDelegate next)
        {
            return context.Request.Method switch
            {
                "GET" => ShiftLockAsync(context, next),
                "DELETE" => ShiftAsync(context, next),
                _ => next(context),
            };
        }

        /// <summary>
        /// Gets the key of the request.
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        string GetKey(HttpContext context)
        {
#if NETCOREAPP3_0
            return context.Request.RouteValues["key"] as string;
#else
            throw new NotImplementedException();
#endif
        }

        Task ShiftLockAsync(HttpContext context, RequestDelegate next)
        {
            context.Request.Headers.TryGetValue("KeyShift-Token", out var token);
            return ShiftLockAsync(context.Response, GetKey(context), token.FirstOrDefault(), context.RequestAborted);
        }

        Task ShiftAsync(HttpContext context, RequestDelegate next)
        {
            context.Request.Headers.TryGetValue("KeyShift-Token", out var token);
            context.Request.Headers.TryGetValue("KeyShift-ForwardUri", out var forwardUri);
            return ShiftAsync(context.Response, GetKey(context), token.FirstOrDefault(), forwardUri.Select(i => new Uri(i)).FirstOrDefault(), context.RequestAborted);
        }

        /// <summary>
        /// Begins a shift, returning the existing data and a token.
        /// </summary>
        /// <param name="key"></param>
        /// <param name="token"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public async Task ShiftLockAsync(HttpResponse response, string key, string token, CancellationToken cancellationToken = default)
        {
            var r = await host.ShiftLockAsync(key, token, cancellationToken);

            // data has already moved
            if (r.ForwardUri != null)
            {
                response.StatusCode = (int)HttpStatusCode.Redirect;
                response.Headers.Add("Location", r.ForwardUri.ToString());
                return;
            }

            // data is present
            if (r.Data != null)
            {
                response.StatusCode = (int)HttpStatusCode.OK;
                response.Headers.Add("KeyShift-Token", r.Token);
                response.ContentType = "application/octet-stream";
                await response.Body.WriteAsync(r.Data, 0, r.Data.Length);
                return;
            }

            response.StatusCode = (int)HttpStatusCode.NotFound;
        }

        /// <summary>
        /// Finalizes a shift, using the existing token.
        /// </summary>
        /// <param name="key"></param>
        /// <param name="token"></param>
        /// <param name="forwardUri"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public async Task ShiftAsync(HttpResponse response, string key, string token, Uri forwardUri, CancellationToken cancellationToken)
        {
            if (token == null)
            {
                response.StatusCode = (int)HttpStatusCode.BadRequest;
                if (response.HttpContext.Features.Get<IHttpResponseFeature>() is IHttpResponseFeature f)
                    f.ReasonPhrase = "Missing KeyShift-Token.";
                return;
            }
            if (forwardUri == null)
            {
                response.StatusCode = (int)HttpStatusCode.BadRequest;
                if (response.HttpContext.Features.Get<IHttpResponseFeature>() is IHttpResponseFeature f)
                    f.ReasonPhrase = "Missing KeyShift-ForwardUri.";
                return;
            }

            await host.ShiftAsync(key, token, forwardUri, cancellationToken);
            response.StatusCode = (int)HttpStatusCode.OK;
        }

    }

}
