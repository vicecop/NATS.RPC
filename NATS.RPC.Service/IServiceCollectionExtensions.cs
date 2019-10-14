using Microsoft.Extensions.DependencyInjection;
using NATS.Client;
using System;
using System.Linq;

namespace NATS.RPC.Service
{
    public static class IServiceCollectionExtensions
    {
        public static IServiceCollection AddNatsRpc(this IServiceCollection serviceDescriptors, Action<IServiceBuilder> builderConfig)
        {
            if (!serviceDescriptors.Any(sd => typeof(ConnectionFactory).IsAssignableFrom(sd.ServiceType)))
                serviceDescriptors.AddSingleton<ConnectionFactory>();

            serviceDescriptors.AddSingleton<ServiceFactory>();

            serviceDescriptors.AddSingleton(provider =>
            {
                var builder = new ServiceBuilder(provider, provider.GetRequiredService<ServiceFactory>());
                builderConfig.Invoke(builder);
                return builder.Build();
            });

            serviceDescriptors.AddHostedService<ServiceHost>();

            return serviceDescriptors;
        }
    }
}
