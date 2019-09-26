using NATS.Client;

namespace NATS.RPC.Console
{
    public interface ITest
    {
        string Echo(string msg);
        void Rpc(string msg, int id);
    }

    internal class Test : ITest
    {
        public string Echo(string msg)
        {
            System.Console.WriteLine($"Echo: {msg}");
            return msg;
        }

        public void Rpc(string msg, int id)
        {
            System.Console.WriteLine($"Rpc: {msg} {id}");
        }
    }

    class Program
    {
        static void Main(string[] args)
        {
            var connectionFactory = new ConnectionFactory();
            var serviceFactory = new ServiceFactory(connectionFactory);
            var test = new Test();

            var service = serviceFactory.Create<ITest, Test>(test, ServiceOptions.Default);

            service.Start();

            var proxyFactory = new ProxyFactory(connectionFactory);
            var proxy = proxyFactory.Create<ITest>(ProxyOptions.Default);

            var response = proxy.Echo("Hello World!");

            System.Console.WriteLine($"Echo response: {response}");

            proxy.Rpc("RPC", 100);
        }
    }
}
