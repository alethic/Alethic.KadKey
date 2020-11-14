using Microsoft.Extensions.DependencyInjection;

namespace Alethic.KeyShift.AspNetCore
{

    public static class ServiceCollectionExtensions
    {

        /// <summary>
        /// Adds the required middleware for the KeyShift process.
        /// </summary>
        /// <param name="services"></param>
        /// <returns></returns>
        public static IServiceCollection AddKeyShiftMiddleware(this IServiceCollection services)
        {
            services.AddTransient<KsHostMiddleware>();
            services.AddTransient<KsKeysMiddleware>();
            return services;
        }

    }

}
