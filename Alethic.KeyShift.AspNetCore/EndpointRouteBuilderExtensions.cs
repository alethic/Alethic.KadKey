#if NETCOREAPP3_0

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace Alethic.KeyShift.AspNetCore
{

    public static class EndpointRouterBuilderExtensions
    {

        /// <summary>
        /// Maps an endpoint for the KeyShift host service.
        /// </summary>
        /// <param name="routes"></param>
        /// <param name="prefix"></param>
        /// <returns></returns>
        public static IEndpointConventionBuilder MapKeyShiftHost(this IEndpointRouteBuilder routes, PathString prefix = default)
        {
            var p = prefix.HasValue ? prefix : new PathString("kshost");
            return routes.Map(p + "/{key}", routes.CreateApplicationBuilder().UseMiddleware<KsHostMiddleware>().Build()).WithDisplayName("KeyShift Host");
        }

        /// <summary>
        /// Maps an endpoint for the KeyShift keys service.
        /// </summary>
        /// <param name="routes"></param>
        /// <param name="prefix"></param>
        /// <returns></returns>
        public static IEndpointConventionBuilder MapKeyShiftKeys(this IEndpointRouteBuilder routes, PathString prefix = default)
        {
            var p = prefix.HasValue ? prefix : new PathString("kshost");
            return routes.Map(p + "/{key}", routes.CreateApplicationBuilder().UseMiddleware<KsKeysMiddleware>().Build()).WithDisplayName("KeyShift Keys");
        }

    }

}

#endif
