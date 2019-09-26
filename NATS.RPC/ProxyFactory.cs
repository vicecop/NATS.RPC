using Castle.DynamicProxy;
using NATS.Client;
using Newtonsoft.Json;
using System.Text;
using System;

namespace NATS.RPC
{
    public class ProxyFactory
    {
        private readonly ConnectionFactory _connectionFactory;
        private readonly ProxyGenerator _proxyGenerator;

        private readonly string _url;

        public ProxyFactory(ConnectionFactory connectionFactory, string url)
        {
            _connectionFactory = connectionFactory ?? throw new ArgumentNullException(nameof(connectionFactory));
            _url = url ?? throw new ArgumentNullException(nameof(url));

            _proxyGenerator = new ProxyGenerator();
        }

        public T Create<T>(string serviceUid)
            where T : class
        {
            var connection = _connectionFactory.CreateConnection(_url);
            var interceptor = new NATSInterceptor(connection, serviceUid ?? throw new ArgumentNullException(nameof(serviceUid)));
            return _proxyGenerator.CreateInterfaceProxyWithoutTarget<T>(interceptor);
        }
    }

    internal class NATSInterceptor : IInterceptor
    {
        private readonly IConnection _connection;
        private readonly string _serviceUid;

        public NATSInterceptor(IConnection connection, string serviceUid)
        {
            _connection = connection;
            _serviceUid = serviceUid;
        }

        public void Intercept(IInvocation invocation)
        {
            try
            {
                var subject = invocation.Method.Name;
                var json = JsonConvert.SerializeObject(invocation.Arguments);
                var bytes = Encoding.UTF8.GetBytes(json);

                var response = _connection.Request($"{_serviceUid}.{subject}", bytes);

                if (invocation.Method.ReturnType == typeof(void))
                    return;

                json = Encoding.UTF8.GetString(response.Data);
                var result = JsonConvert.DeserializeObject(json, invocation.Method.ReturnType);

                invocation.ReturnValue = result;
            }
            catch
            {
                invocation.ReturnValue = null;
            }
        }
    }
}
