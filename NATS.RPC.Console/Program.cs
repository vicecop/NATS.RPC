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
            var url = "nats://localhost:4222";
            var serviceUid = "Test";
            //var serviceUid1 = "Test1";

            var serviceFactory = new ServiceFactory(connectionFactory, url);
            var test = new Test();

            serviceFactory.Create<ITest, Test>(test, serviceUid);
            //serviceFactory.Create<ITest, Test>(test, serviceUid1);

            var proxyFactory = new ProxyFactory(connectionFactory, url);
            var proxy = proxyFactory.Create<ITest>(serviceUid);
            //var proxy1 = proxyFactory.Create<ITest>(serviceUid1);

            var response = proxy.Echo("Hello World!");

            System.Console.WriteLine($"Echo response: {response}");

            proxy.Rpc("RPC", 100);

            //response = proxy1.Echo("!@#");

            //System.Console.WriteLine($"Echo response: {response}");

        }
    }
}
