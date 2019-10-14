using Microsoft.Extensions.DependencyInjection;
using NATS.Client;
using System;

namespace NATS.RPC.Service
{
    public interface IServiceBuilder
    {
        ServiceBuilder Configure(Action<ServiceOptions> options);
        ServiceBuilder AddMsgEventHandler(EventHandler<MsgHandlerEventArgs> eventHandler);
        ServiceBuilder AddContractHandler<TContract, TImplementation>(Func<IServiceProvider, ObjectFactory> factory = null)
            where TContract : class
            where TImplementation : class, TContract;
    }
}
