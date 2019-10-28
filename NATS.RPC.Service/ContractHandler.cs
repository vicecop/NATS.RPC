using Microsoft.Extensions.DependencyInjection;
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
        private readonly IServiceProvider _serviceProvider;
        private readonly ObjectFactory _contractImplFactory;

        public string BaseRoute { get; }
        public string HandlerRoute { get; }
        public Type ContractType { get; }
        public object ContractImplementaion { get; }

        public ContractHandler(IServiceProvider serviceProvider, Type contractType, 
            string baseRoute, ObjectFactory contractImplFactory)
        {
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
                    var json = Encoding.UTF8.GetString(args.Message.Data);
                    var jToken = JToken.Parse(json);

                    var parameters = method.GetParameters();
                    var arguments = new object[parameters.Length];

                    for (int i = 0; i < parameters.Length; i++)
                    {
                        arguments[i] = jToken[i].ToObject(parameters[i].ParameterType);
                    }

                    object result;

                    var scope = _serviceProvider.CreateScope();

                    var contractImplementaion = _contractImplFactory.Invoke(scope.ServiceProvider, Array.Empty<object>());
                    result = method.Invoke(contractImplementaion, arguments);

                    if (typeof(Task).IsAssignableFrom(method.ReturnType))
                    {
                        var task = (Task)result;

                        await task;

                        scope.Dispose();

                        var prop = method.ReturnType.GetProperty("Result");
                        result = prop?.GetValue(task);
                    }

                    scope.Dispose();

                    json = JsonConvert.SerializeObject(result);
                    var bytes = Encoding.UTF8.GetBytes(json);

                    connection.Publish(args.Message.Reply, bytes);
                };

                yield return subscription;
            }
        }
    }
}
