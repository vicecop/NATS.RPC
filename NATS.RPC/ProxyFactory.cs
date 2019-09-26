using Castle.DynamicProxy;
using NATS.Client;
using System;

namespace NATS.RPC
{
    public class ProxyFactory
    {
        private readonly ConnectionFactory _connectionFactory;
        private readonly ProxyGenerator _proxyGenerator;

        public ProxyFactory(ConnectionFactory connectionFactory)
        {
            _connectionFactory = connectionFactory ?? throw new ArgumentNullException(nameof(connectionFactory));
            _proxyGenerator = new ProxyGenerator();
        }

        public T Create<T>(ProxyOptions options)
            where T : class
        {
            if (options == null)
                throw new ArgumentNullException(nameof(options));

            var connection = _connectionFactory.CreateConnection(options.ConnectionString);
            var interceptor = new ContractInterceptor(connection, typeof(T), options.ServiceUid);
            return _proxyGenerator.CreateInterfaceProxyWithoutTarget<T>(interceptor);
        }
    }
}
