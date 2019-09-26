using NATS.Client;
using System;

namespace NATS.RPC
{
    public class ServiceFactory
    {
        private readonly ConnectionFactory _connectionFactory;

        public ServiceFactory(ConnectionFactory connectionFactory)
        {
            _connectionFactory = connectionFactory;
        }

        public Service Create<TContract, TImplementation>(TImplementation contractImplmentation, 
            ServiceOptions options, EventHandler<MsgHandlerEventArgs> msgHandler = null)
            where TImplementation : TContract
        {
            return new Service(_connectionFactory, typeof(TContract), contractImplmentation, options, msgHandler);
        }
    }
}
