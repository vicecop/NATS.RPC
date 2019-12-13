using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NATS.Client;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Text;
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

        public ContractHandler(ILogger<ContractHandler> logger, IServiceProvider serviceProvider, Type contractType, 
            string baseRoute, ObjectFactory contractImplFactory)
        {
            _logger = logger;
            _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));

            ContractType = contractType ?? throw new ArgumentNullException(nameof(contractType));
            BaseRoute = baseRoute ?? throw new ArgumentNullException(nameof(baseRoute));
            HandlerRoute = $"{BaseRoute}.{ContractType.Name}";

            _contractImplFactory = contractImplFactory ?? throw new ArgumentNullException(nameof(contractImplFactory));
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
                        var json = Encoding.UTF8.GetString(args.Message.Data);
                        var jToken = JToken.Parse(json);

                        var parameters = method.GetParameters();
                        var arguments = new object[parameters.Length];

                        for (int i = 0; i < parameters.Length; i++)
                        {
                            arguments[i] = jToken[i].ToObject(parameters[i].ParameterType);
                        }

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

                        json = JsonConvert.SerializeObject(result);
                        var bytes = Encoding.UTF8.GetBytes(json);

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
