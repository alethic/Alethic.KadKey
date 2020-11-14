using System.Threading.Tasks;

using Alethic.KeyShift.AspNetCore;

using Autofac.Extensions.DependencyInjection;

using Cogito.Autofac;
using Cogito.Extensions.Options.Autofac;
using Cogito.Extensions.Options.Configuration.Autofac;
using Cogito.Serilog;

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Alethic.KeyShift.Console
{

    public static class Program
    {

        [RegisterAs(typeof(ILoggerConfigurator))]
        public class LoggerConfigurator : ILoggerConfigurator
        {

            public global::Serilog.LoggerConfiguration Apply(global::Serilog.LoggerConfiguration configuration)
            {
                return configuration.MinimumLevel.Verbose();
            }

        }

        /// <summary>
        /// Main application entry point.
        /// </summary>
        /// <param name="args"></param>
        /// <returns></returns>
        public static async Task Main(string[] args) =>
            await Host.CreateDefaultBuilder(args)
                .UseServiceProviderFactory(new AutofacServiceProviderFactory(b => b.RegisterAllAssemblyModules()))
                .ConfigureHostConfiguration(b => b.AddEnvironmentVariables().AddCommandLine(args))
                .ConfigureWebHostDefaults(w => w
                    .ConfigureServices(s => s
                        .AddKeyShiftMiddleware()
                        .AddMvc().AddControllersAsServices())
                    .Configure(c => c
                        .UseRouting()
                        .UseEndpoints(e =>
                        {
                            e.MapKeyShiftHost("/host");
                            e.MapKeyShiftKeys("/keys");
                        })))
                .RunConsoleAsync();

    }

}
