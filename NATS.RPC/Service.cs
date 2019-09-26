using NATS.Client;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Text;

namespace NATS.RPC
{
    public class Service
    {
        private readonly ConnectionFactory _connectionFactory;

        private IConnection _connection;
        private IEnumerable<IAsyncSubscription> _subscriptions;

        public string ServiceUid { get; }
        public string ConnectionString { get; }
        public Type ContractType { get; }
        public object ContractImplementaion { get; }

        public EventHandler<MsgHandlerEventArgs> MsgHandler { get; }

        public Service(ConnectionFactory connectionFactory, Type contractType, 
            object contractImplementation, ServiceOptions options, EventHandler<MsgHandlerEventArgs> eventHandler = null)
        {
            _connectionFactory = connectionFactory ?? throw new ArgumentNullException(nameof(connectionFactory));

            ContractType = contractType ?? throw new ArgumentNullException(nameof(contractType));
            ContractImplementaion = contractImplementation ?? throw new ArgumentNullException(nameof(contractImplementation));

            ServiceUid = options?.ServiceUid ?? throw new ArgumentNullException(nameof(ServiceUid));
            ConnectionString = options?.ConnectionString ?? throw new ArgumentNullException(nameof(ConnectionString));

            MsgHandler = eventHandler;
        }

        public void Start()
        {
            _connection = _connectionFactory.CreateConnection(ConnectionString);

            var subscriptions = new List<IAsyncSubscription>();
            foreach(var method in ContractType.GetMethods())
            {
                var subscription = _connection.SubscribeAsync($"{ServiceUid}.{ContractType.Name}.{method.Name}");
                subscription.MessageHandler += (sender, args) =>
                {
                    var json = Encoding.UTF8.GetString(args.Message.Data);
                    var jToken = JToken.Parse(json);

                    var parameters = method.GetParameters();
                    var arguments = new object[parameters.Length];

                    for (int i = 0; i < parameters.Length; i++)
                    {
                        arguments[i] = jToken[i].ToObject(parameters[i].ParameterType);
                    }

                    var result = method.Invoke(ContractImplementaion, arguments);

                    json = JsonConvert.SerializeObject(result);
                    var bytes = Encoding.UTF8.GetBytes(json);

                    _connection.Publish(args.Message.Reply, bytes);
                };

                if (MsgHandler != null)
                    subscription.MessageHandler += MsgHandler;

                subscriptions.Add(subscription);
            }

            _subscriptions = subscriptions;

            foreach (var sub in _subscriptions)
                sub.Start();
        }

        public void Stop()
        {
            _connection.Close();
        }
    }
}
