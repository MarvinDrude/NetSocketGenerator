
# Fast Tcp Socket Library

Lightweight library for TCP Communication in .NET 10 / C#. Still work in progress.

## Supported features

- Blazingly fast 🚀
- Easy to use and setup 🪄
- Defaults to fast sockets but supports stream and ssl stream 🔒
- High-throughput batching 📈
- Custom framing 🛠️
- Add event handlers with wildcards like *,[abc], ? and escape with \\ ✳️
- Source-generator–powered handler wiring via IServiceProvider ✨
- In-Memory Message Bus Sender / Producer with acknowledgement 📬
- In-Memory Key Value Store 🗄️

## Next features

- More Functions and Types for Key Value Store ⚙️
- Simple Clustering support for CacheQueue 🌐
- More Tests 🧪

## Project Overview

| Project/Package name | Description                |
| :-------- | :------------------------- |
| NetSocketGenerator | Base fast Tcp Server/Client functionality, including small source generator |
| NetSocketGenerator.CacheQueue | Simple In-Memory Key-Value Store with Subscribe Queue capability and clustering |

## Nuget

| Project/Package name | Nuget                |
| :-------- | :------------------------- |
| NetSocketGenerator | [![Nuget](https://img.shields.io/badge/nuget-0A66C2?style=for-the-badge&logo=nuget&logoColor=white)](https://www.nuget.org/packages/NetSocketGenerator) |
| NetSocketGenerator.CacheQueue (Server) | [![Nuget](https://img.shields.io/badge/nuget-0A66C2?style=for-the-badge&logo=nuget&logoColor=white)](https://www.nuget.org/packages/NetSocketGenerator.CacheQueue) |
| NetSocketGenerator.CacheQueue.Client | [![Nuget](https://img.shields.io/badge/nuget-0A66C2?style=for-the-badge&logo=nuget&logoColor=white)](https://www.nuget.org/packages/NetSocketGenerator.CacheQueue.Client) |

## Bare bones CacheQueue example

Starting a standalone none-cluster CacheQueue Server 
```C#
// Can totally be used with the ASP.NET Core builder too
ServiceCollection collection = new();
collection.AddCacheQueue();

var serviceProvider = collection.BuildServiceProvider();

var localCacheQueue = new CacheQueueServer(
   serviceProvider.GetRequiredService<ILogger<CacheQueueServer>>(),
   new CacheQueueServerOptions()
   {
      ServiceProvider = serviceProvider,
      Address = "127.0.0.1",
      Port = 34444,
   });
localCacheQueue.Start();
```

Client creation (auto reconnects)
```C#
// Can totally be used with the ASP.NET Core builder too
collection.AddCacheQueueClient();

var testClient = new CacheQueueClient(new CacheQueueClientOptions()
{
   Address = "127.0.0.1",
   Port = 34444,
   ServiceProvider = serviceProvider
});
testClient.Connect();
```

Queue example (Publishing and handler can be on totally different clients)
```C#
await testClient.Queue.Create(queueName);
await testClient.Queue.Subscribe(queueName);

testClient.Queue.AddHandler<TestMessage>(queueName, async (context) =>
{
   // can respond if required in PublishAndReceive
   await context.Respond(new TestMessageBad() { Message = "comeback" });
});

testClient.Queue.PublishNoAck(queueName, new TestMessage() { Message = "test1" });
await testClient.Queue.Publish(queueName, new TestMessage() { Message = "test5" });
var tt = await testClient.Queue.PublishAndReceive<TestMessage, TestMessageBad>(queueName, new TestMessage() { Message = "test4" });
```

KeyValue example
```C#
await client.Integers.Set(keyOne, 2000);
await client.Doubles.Add(keyOne, 130d);
await client.Longs.Increment(keyOne);
await client.Strings.Set(keyOne, "test");

var t = await client.Strings.Get(keyOne);
```
Batching
```C#
var batch = await client
   .CreateBatch()
   .Doubles.Set(keyOne, 10d)
   .Doubles.Add(keyOne, 5d)
   .Doubles.Subtract(keyOne, 2d)
   .Doubles.Increment(keyOne)
   .Doubles.Decrement(keyOne)
   .Integers.Set(keyTwo, 20000)
   .Send();

// Add/Subtract/Increment/Decrement are all add commands behind the scenes
batch.GetAck<AddDoubleCommandAck>(4)!.NewValue should be 13
```

## Bare bones NetSocketGenerator example

```C#
// Can totally be used with the ASP.NET Core builder too
var collection = new ServiceCollection();

// Just register all processors found in this project
collection.AddSocketClientProcessors();
collection.AddSocketServerProcessors();

// Also available in ASP.NET Core
var serviceProvider = collection.BuildServiceProvider();

// Create server and start the server
var server = new TcpServer(new TcpServerOptions()
{
   Address = "127.0.0.1",
   Port = 34533,
   ServiceProvider = serviceProvider,
});
server.UseSocketServerProcessors(); // Use all processors
server.Start();

// Create a client and connect to the server
var client = new TcpClient(new TcpClientOptions()
{
   Address = "127.0.0.1",
   Port = 34533,
   ServiceProvider = serviceProvider,
   Events = new TcpEventCallbacks()
   {
      OnConnected = (connection) =>
      {
         connection.Send("ping:test", new PingProcessor.PingMessage());
         return Task.CompletedTask;
      }
   }
});
client.UseSocketClientProcessors();
client.Connect();

// Create a partial class as processor for incoming ping messages
[SocketProcessor(
   EventNamePattern = "ping:*"
)]
public sealed partial class PingProcessor
{
   public PingProcessor()
   {
      
   }

   public async Task Execute(
      ITcpConnection connection,
      [SocketEventName] string eventName,
      [SocketPayload] PingMessage payload)
   {
      connection.Send("pong:" + "AA", new PongProcessor.PongMessage()
      {
         
      });
      Console.WriteLine("ping");
      await Task.CompletedTask;
   }

   public sealed class PingMessage
   {
      
   }
}

// create a partial class as processor for incoming pong messages
[SocketProcessor(
   EventNamePattern = "pong:*",
   RegistrationGroups = ["System"]
)]
public sealed partial class PongProcessor
{
   public PongProcessor()
   {
      
   }

   public async Task Execute(
      ITcpConnection connection,
      [SocketEventName] string eventName,
      [SocketPayload] PongMessage payload,
      PingProcessor testService)
   {
      connection.Send("ping:" + "AA", new PingProcessor.PingMessage()
      {
         
      });
      
      Console.WriteLine("pong");
      await Task.CompletedTask;
   }

   public sealed class PongMessage
   {
      
   }
}
```
