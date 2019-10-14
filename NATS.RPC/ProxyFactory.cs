using NATS.Client;
using Newtonsoft.Json;
using SexyProxy;
using System;
using System.Linq;
using System.Text;

namespace NATS.RPC.Proxy
{
    public class ProxyFactory
    {
        private readonly ConnectionFactory _connectionFactory;

        public ProxyFactory(ConnectionFactory connectionFactory)
        {
            _connectionFactory = connectionFactory ?? throw new ArgumentNullException(nameof(connectionFactory));
        }

        public T Create<T>(ProxyOptions options)
            where T : class
        {
            if (options == null)
                throw new ArgumentNullException(nameof(options));

            var connection = _connectionFactory.CreateConnection(options.ConnectionString);

            return SexyProxy.Proxy.CreateProxy<T>(async invocation =>
            {
                var json = JsonConvert.SerializeObject(invocation.Arguments);
                var bytes = Encoding.UTF8.GetBytes(json);
                var subject = $"{options.ServiceUid}.{typeof(T).Name}.{invocation.Method.Name}";

                var response = await connection.RequestAsync(subject, bytes);

                if (invocation.Method.ReturnType == typeof(void))
                    return null;

                json = Encoding.UTF8.GetString(response.Data);

                Type type;

                if (invocation.HasFlag(InvocationFlags.Async))
                {
                    if (invocation.Method.ReturnType.IsGenericType)
                    {
                        type = invocation.Method.ReturnType.GetGenericArguments().Single();
                    }
                    else
                    {
                        type = typeof(object);
                    }
                }
                else
                {
                    type = invocation.Method.ReturnType;
                }

                var result = JsonConvert.DeserializeObject(json, type);
                return result;
            }, asyncMode: AsyncInvocationMode.Wait);
        }
    }
}
