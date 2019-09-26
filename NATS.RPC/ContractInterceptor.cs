using Castle.DynamicProxy;
using NATS.Client;
using Newtonsoft.Json;
using System;
using System.Text;

namespace NATS.RPC
{
    internal class ContractInterceptor : IInterceptor
    {
        private readonly IConnection _connection;
        private readonly Type _contractType;
        private readonly string _serviceUid;

        public ContractInterceptor(IConnection connection, Type contractType, string serviceUid)
        {
            _connection = connection ?? throw new ArgumentNullException(nameof(connection));
            _contractType = contractType ?? throw new ArgumentNullException(nameof(contractType));
            _serviceUid = serviceUid ?? throw new ArgumentNullException(nameof(serviceUid));
        }

        public void Intercept(IInvocation invocation)
        {
            try
            {
                var json = JsonConvert.SerializeObject(invocation.Arguments);
                var bytes = Encoding.UTF8.GetBytes(json);

                var response = _connection.Request($"{_serviceUid}.{_contractType.Name}.{invocation.Method.Name}", bytes);

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
