﻿using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using NATS.Client;
using NATS.RPC.Proxy;
using NATS.RPC.Service;
using System.Threading.Tasks;

namespace NATS.RPC.Console
{
    public interface ITest
    {
        string Echo(string msg);
        void Rpc(string msg, int id);

        Task<string> EchoAsync(string msg);
        Task RpcAsync(string msg, int id);

        Task<TestModel> EchoModel(TestModel testModel);

        public class TestModel
        {
            public string Name { get; set; }
        }
    }

    internal class Test : ITest
    {
        public string Echo(string msg)
        {
            System.Console.WriteLine($"Echo: {msg}");
            return msg;
        }

        public Task<string> EchoAsync(string msg)
        {
            return Task.FromResult(Echo(msg));
        }

        public Task<ITest.TestModel> EchoModel(ITest.TestModel testModel)
        {
            return Task.FromResult(testModel);
        }

        public void Rpc(string msg, int id)
        {
            System.Console.WriteLine($"Rpc: {msg} {id}");
        }

        public Task RpcAsync(string msg, int id)
        {
            Rpc(msg, id);
            return Task.CompletedTask;
        }
    }

    class Program
    {
        static void Main(string[] args)
        {
            var serviceCollection = new ServiceCollection();
            serviceCollection.AddLogging();
            serviceCollection.AddNatsRpc(config => config.AddContractHandler<ITest, Test>());
            var provider = serviceCollection.BuildServiceProvider();
            var serviceHost = provider.GetRequiredService<IHostedService>();
            var cts = new System.Threading.CancellationTokenSource();
            var token = cts.Token;
            serviceHost.StartAsync(token);

            var connectionFactory = new ConnectionFactory();
            var proxyFactory = new ProxyFactory(connectionFactory);
            var proxy = proxyFactory.Create<ITest>(ProxyOptions.Default);

            var response = proxy.Echo("Hello World!");

            System.Console.WriteLine($"Echo response: {response}");

            proxy.Rpc("RPC", 100);

            var responseTask = proxy.EchoAsync("Hello World Async!");

            System.Console.WriteLine($"Echo response: {responseTask.GetAwaiter().GetResult()}");

            proxy.RpcAsync("Async RPC", 101).GetAwaiter().GetResult();

            var model = new ITest.TestModel()
            {
                Name = "TEST"
            };

            var echoModel = proxy.EchoModel(model).Result;

            System.Console.WriteLine($"Async echo model: {echoModel.Name}");

            System.Console.ReadLine();

            cts.Cancel();
            cts.Dispose();
        }
    }
}
