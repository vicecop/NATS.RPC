# NATS.RPC

**WORK IN PROGRESS**

Lightweight RPC-over-[NATS](https://nats.io/) realization, based on contracts (native .NET interfaces) with runtime service proxy generation for .NET Core.

**Target framework:**
+ netstandard 2.1
  
**Dependencies:**
+ Microsoft.Extensions.DependencyInjection.Abstractions 3.0.0
+ Microsoft.Extensions.Hosting.Abstractions 3.0.0
+ Castle.Core 4.4.0
+ NATS.Client 0.9.0
+ Newtonsoft.Json 12.0.2

---

## Example

**Contract:**
```C#
  public interface ITest
  {
      //Sync
      string Echo(string msg);
      void Rpc(string msg, int id);
      
      //Async
      Task<string> EchoAsync(string msg);
      Task RpcAsync(string msg, int id);
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
      
      public Task<string> EchoAsync(string msg)
      {
          return Task.FromResult(Echo(msg));
      }
        
      public Task RpcAsync(string msg, int id)
      {
          Rpc(msg, id);
          return Task.CompletedTask;
      }
  }
```

**Adding to DI-container**
```C#
  void Configure(IServiceCollection services)
  {
      services.AddNatsRpc(config => 
      {
          config.AddContractHandler<ITest, Test>());
      }
  }
```

**Proxy creation:**
```C#
  var proxyFactory = new ProxyFactory(connectionFactory);
  var proxy = proxyFactory.Create<ITest>(ProxyOptions.Default);
```

**Proxy usage:**
```C#
  //Sync
  var response = proxy.Echo("Hello World!");

  System.Console.WriteLine($"Echo response: {response}");

  proxy.Rpc("RPC", 100);
  
  //Async
  await proxy.RpcAsync("Async RPC", 101)
  
  response = await proxy.EchoAsync("Hello World Async!");

  System.Console.WriteLine($"Echo async response: {response}");
```
