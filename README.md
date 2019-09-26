# NATS.RPC

**WORK IN PROGRESS**

Lightweight .NET C# RPC-over-NATS realization, based on contracts (native .NET interfaces) with runtime service proxy generation.

**Target framework:**
+ netstandard2.0
  
**Dependencies:**
+ Castle.Core 4.4.0
+ NATS.Client 0.9.0
+ Newtonsoft.Json 12.0.2

---

## Example

**Contract:**
```C#
  public interface ITest
  {
      string Echo(string msg);
      void Rpc(string msg, int id);
  }
```

**Contract implementation (Service):**
```C#
  public class Test : ITest
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
```

**Service creation:**
```C#
  var connectionFactory = new ConnectionFactory();
  var serviceFactory = new ServiceFactory(connectionFactory);
  var test = new Test();

  var service = serviceFactory.Create<ITest, Test>(test, ServiceOptions.Default);

  service.Start();
  
  service.Stop();
```

**Proxy creation:**
```C#
  var proxyFactory = new ProxyFactory(connectionFactory);
  var proxy = proxyFactory.Create<ITest>(ProxyOptions.Default);
```

**Proxy usage:**
```C#
  var response = proxy.Echo("Hello World!");

  System.Console.WriteLine($"Echo response: {response}");

  proxy.Rpc("RPC", 100);
```
