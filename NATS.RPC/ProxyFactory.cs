using NATS.Client;
using NATS.RPC.Shared;
using SexyProxy;
using System;
using System.Buffers;
using System.IO;
using System.Linq;

namespace NATS.RPC.Proxy
{
    public sealed class ProxyFactory
    {
        private readonly ConnectionFactory _connectionFactory;

        public ProxyFactory(ConnectionFactory connectionFactory)
        {
            _connectionFactory = connectionFactory ?? throw new ArgumentNullException(nameof(connectionFactory));
        }

        public T Create<T>(ISerializer serializer, IDeserializer deserialzer, ProxyOptions options)
            where T : class, IDisposable
        {
            if (options == null)
                throw new ArgumentNullException(nameof(options));

            var connection = _connectionFactory.CreateConnection(options.ConnectionString);
            var arrayPool = ArrayPool<byte>.Shared;

            return SexyProxy.Proxy.CreateProxy<T>(async invocation =>
            {
                if (invocation.Method.Name == "Dispose")
                {
                    connection.Close();
                    return null;
                }

                var parameters = invocation.Method.GetParameters();

                var argBytes = serializer.SerializeObjects(invocation.Arguments);
                var subject = $"{options.ServiceUid}.{typeof(T).Name}.{invocation.Method.Name}";

                var response = await connection.RequestAsync(subject, argBytes, options.TimeoutMs);

                if (invocation.Method.ReturnType == typeof(void))
                    return null;

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

                var result = deserialzer.DeserialzeObject(response.Data, type);

                return result;
            }, asyncMode: AsyncInvocationMode.Wait);
        }

        public T Create<T>(ProxyOptions options)
            where T : class, IDisposable
        {
            var serializer = new Shared.JsonSerializer();
            var deserializer = new JsonDeserializer();
            return Create<T>(serializer, deserializer, options);
        }
    }
}
