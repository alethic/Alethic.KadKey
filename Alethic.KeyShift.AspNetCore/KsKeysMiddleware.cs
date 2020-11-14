using System;
using System.Net;
using System.Threading.Tasks;

using Cogito.IO;

using Microsoft.AspNetCore.Http;

namespace Alethic.KeyShift.AspNetCore
{

    /// <summary>
    /// Implements the KeyShift frontend hosting APIs.
    /// </summary>
    public class KsKeysMiddleware : IMiddleware
    {

        readonly IKsHost<string> host;

        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        /// <param name="host"></param>
        public KsKeysMiddleware(IKsHost<string> host)
        {
            this.host = host ?? throw new ArgumentNullException(nameof(host));
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

        public Task InvokeAsync(HttpContext context, RequestDelegate next)
        {
            return context.Request.Method switch
            {
                "GET" => GetAsync(context, next),
                "PUT" => SetAsync(context, next),
                _ => next(context),
            };
        }

        async Task GetAsync(HttpContext context, RequestDelegate next)
        {
            var data = await host.GetAsync(GetKey(context), context.RequestAborted);
            if (data == null)
            {
                context.Response.StatusCode = (int)HttpStatusCode.NotFound;
                return;
            }

            context.Response.StatusCode = (int)HttpStatusCode.OK;
            context.Response.ContentType = "application/octet-stream";
            await context.Response.Body.WriteAsync(data, 0, data.Length);
        }

        async Task SetAsync(HttpContext context, RequestDelegate next)
        {
            await host.SetAsync(GetKey(context), await context.Request.Body.ReadAllBytesAsync(), context.RequestAborted);
            context.Response.StatusCode = (int)HttpStatusCode.OK;
        }

    }

}
