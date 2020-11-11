using Autofac;
using Autofac.Extensions.DependencyInjection;

using Cogito.Autofac;
using Cogito.Autofac.DependencyInjection;
using Cogito.Extensions.Options.Autofac;
using Cogito.Extensions.Options.Configuration.Autofac;
using Cogito.Kademlia;
using Cogito.Kademlia.InMemory;
using Cogito.Kademlia.Json;
using Cogito.Kademlia.MessagePack;
using Cogito.Kademlia.Network.Udp;
using Cogito.Kademlia.Protobuf;

using Microsoft.Extensions.DependencyInjection;

namespace Alethic.KeyShift.Console
{

    public class AssemblyModule : ModuleBase
    {

        protected override void Register(ContainerBuilder builder)
        {
            builder.RegisterFromAttributes(typeof(AssemblyModule).Assembly);
            builder.Populate(s => s.AddHttpClient());
            RegisterKademlia(builder, 40512);
            RegisterKeyShift(builder);
        }

        static void RegisterKademlia(ContainerBuilder builder, ulong networkId)
        {
            builder.RegisterType<KJsonMessageFormat<KNodeId256>>().AsImplementedInterfaces().SingleInstance();
            builder.RegisterType<KProtobufMessageFormat<KNodeId256>>().AsImplementedInterfaces().SingleInstance();
            builder.RegisterType<KMessagePackMessageFormat<KNodeId256>>().AsImplementedInterfaces().SingleInstance();
            builder.RegisterType<KRefresher<KNodeId256>>().AsImplementedInterfaces().SingleInstance();
            builder.RegisterType<KConnector<KNodeId256>>().AsImplementedInterfaces().SingleInstance();
            builder.RegisterType<KInvoker<KNodeId256>>().AsImplementedInterfaces().SingleInstance();
            builder.RegisterType<KInvokerPolicy<KNodeId256>>().AsImplementedInterfaces().SingleInstance();
            builder.RegisterType<KRequestHandler<KNodeId256>>().AsImplementedInterfaces().SingleInstance();
            builder.RegisterType<KFixedTableRouter<KNodeId256>>().AsImplementedInterfaces().SingleInstance();
            builder.RegisterType<KLookup<KNodeId256>>().AsImplementedInterfaces().SingleInstance();
            builder.RegisterType<KValueAccessor<KNodeId256>>().AsImplementedInterfaces().SingleInstance();
            builder.RegisterType<KInMemoryStore<KNodeId256>>().AsImplementedInterfaces().SingleInstance();
            builder.RegisterType<KInMemoryPublisher<KNodeId256>>().AsImplementedInterfaces().SingleInstance();
            builder.RegisterType<KHost<KNodeId256>>().AsImplementedInterfaces().SingleInstance();
            builder.RegisterType<KUdpProtocol<KNodeId256>>().AsImplementedInterfaces().SingleInstance();
            builder.RegisterType<KUdpMulticastDiscovery<KNodeId256>>().AsImplementedInterfaces().SingleInstance();
            builder.Configure<KHostOptions<KNodeId256>>(o => o.NetworkId = networkId);
            builder.Configure<KFixedTableRouterOptions>(o => { });
        }

        static void RegisterKeyShift(ContainerBuilder builder)
        {
            builder.RegisterType<KsHostHttpClientFactory<string>>().AsImplementedInterfaces().SingleInstance();
            builder.RegisterType<KsHostClientProvider<string>>().AsImplementedInterfaces().SingleInstance();
            builder.RegisterType<KsHost<string>>().AsImplementedInterfaces().SingleInstance();
            builder.RegisterType<KsStringHash>().AsImplementedInterfaces().SingleInstance();
            builder.Configure<KsHostOptions>("Alethic.KeyShift:KeyStore");
        }

    }

}
