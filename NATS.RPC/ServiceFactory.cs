using NATS.Client;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NATS.RPC
{
    public class ServiceFactory
    {
        private readonly ConnectionFactory _connectionFactory;
        private readonly string _url;

        public ServiceFactory(ConnectionFactory connectionFactory, string url)
        {
            _connectionFactory = connectionFactory;
            _url = url;
        }

        public IEnumerable<IAsyncSubscription> Create<TInterface, TRealization>(TRealization realization, string serviceUid)
            where TRealization : TInterface
        {
            var connection = _connectionFactory.CreateConnection(_url);
            return typeof(TInterface).GetMethods().Select(method => 
            connection.SubscribeAsync($"{serviceUid}.{method.Name}", (sender, args) =>
            {
                var json = Encoding.UTF8.GetString(args.Message.Data);
                var jToken = JToken.Parse(json);

                var parameters = method.GetParameters();
                var arguments = new object[parameters.Length];

                for (int i = 0; i < parameters.Length; i++)
                {
                    arguments[i] = jToken[i].ToObject(parameters[i].ParameterType);
                }

                var result = method.Invoke(realization, arguments);

                json = JsonConvert.SerializeObject(result);
                var bytes = Encoding.UTF8.GetBytes(json);

                connection.Publish(args.Message.Reply, bytes);
            })).ToArray();
        }
    }
}
