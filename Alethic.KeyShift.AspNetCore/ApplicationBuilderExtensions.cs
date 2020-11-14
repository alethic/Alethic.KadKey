
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;

namespace Alethic.KeyShift.AspNetCore
{

    public static class ApplicationBuilderExtensions
    {

        /// <summary>
        /// Maps an endpoint for the KeyShift host service.
        /// </summary>
        /// <param name="app"></param>
        /// <param name="prefix"></param>
        /// <returns></returns>
        public static IApplicationBuilder MapKeyShiftHost(this IApplicationBuilder app, PathString prefix = default)
        {
            var p = prefix.HasValue ? prefix : new PathString("kshost");
            return app.Map(p, a => a.UseMiddleware<KsHostMiddleware>().Build());
        }

        /// <summary>
        /// Maps an endpoint for the KeyShift keys service.
        /// </summary>
        /// <param name="app"></param>
        /// <param name="prefix"></param>
        /// <returns></returns>
        public static IApplicationBuilder MapKeyShiftKeys(this IApplicationBuilder app, PathString prefix = default)
        {
            var p = prefix.HasValue ? prefix : new PathString("kshost");
            return app.Map(p, a => a.UseMiddleware<KsKeysMiddleware>().Build());
        }

    }

}
