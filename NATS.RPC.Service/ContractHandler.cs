using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NATS.Client;
using NATS.RPC.Shared;
using System;
using System.Buffers;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace NATS.RPC.Service
{
    public class ContractHandler
    {
        private readonly ILogger _logger;

        private readonly IServiceProvider _serviceProvider;
        private readonly ObjectFactory _contractImplFactory;

        public string BaseRoute { get; }
        public string HandlerRoute { get; }
        public Type ContractType { get; }
        public object ContractImplementaion { get; }

        private ISerializer _serializer;
        private IDeserializer _deserialzer;

        public ContractHandler(ILogger<ContractHandler> logger, IServiceProvider serviceProvider, 
            ISerializer serializer, IDeserializer deserialzer, Type contractType, 
            string baseRoute, ObjectFactory contractImplFactory)
        {
            _logger = logger;
            _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));

            ContractType = contractType ?? throw new ArgumentNullException(nameof(contractType));
            BaseRoute = baseRoute ?? throw new ArgumentNullException(nameof(baseRoute));
            HandlerRoute = $"{BaseRoute}.{ContractType.Name}";

            _contractImplFactory = contractImplFactory ?? throw new ArgumentNullException(nameof(contractImplFactory));

            _serializer = serializer ?? throw new ArgumentNullException(nameof(serializer));
            _deserialzer = deserialzer ?? throw new ArgumentNullException(nameof(deserialzer));
        }

        public IEnumerable<IAsyncSubscription> Subscribe(IConnection connection)
        {
            foreach (var method in ContractType.GetMethods())
            {
                var methodRoute = $"{HandlerRoute}.{method.Name}";
                var subscription = connection.SubscribeAsync(methodRoute);

                subscription.MessageHandler += async (sender, args) =>
                {
                    try
                    {
                        var parameterTypes = method.GetParameters().Select(p => p.ParameterType).ToArray();
                        var arguments = _deserialzer.DeserialzeObjects(args.Message.Data, parameterTypes);

                        object result;

                        using (var scope = _serviceProvider.CreateScope())
                        {
                            var contractImplementaion = _contractImplFactory.Invoke(scope.ServiceProvider, Array.Empty<object>());
                            result = method.Invoke(contractImplementaion, arguments);

                            if (typeof(Task).IsAssignableFrom(method.ReturnType))
                            {
                                var task = (Task)result;

                                await task;

                                var prop = method.ReturnType.GetProperty("Result");
                                result = prop?.GetValue(task);
                            }
                        }

                        var bytes = _serializer.SerializeObject(result);

                        connection.Publish(args.Message.Reply, bytes);
                    }
                    catch(Exception ex)
                    {
                        _logger?.LogError(ex, "Unhandled exception during rpc-request has occured");
                    }
                };

                yield return subscription;
            }
        }
    }
}
