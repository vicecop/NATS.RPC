using NATS.Client;
using System;
using System.Collections.Generic;

namespace NATS.RPC.Service
{
    public class ServiceFactory
    {
        private readonly IServiceProvider _serviceProvider;

        public ServiceFactory(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public Service Create(ServiceOptions options, IEnumerable<ContractHandler> contractHandlers, 
            EventHandler<MsgHandlerEventArgs> msgHandler = null)
        {
            return new Service(_serviceProvider, contractHandlers, options, msgHandler);
        }
    }
}
