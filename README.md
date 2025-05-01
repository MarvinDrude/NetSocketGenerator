
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


## Next features

- Simple Message Bus capabilities 🔀
- Maybe simple clustering with hash slots 🌍

## Project Overview

| Project/Package name | Description                |
| :-------- | :------------------------- |
| NetSocketGenerator | Base fast Tcp Server/Client functionality, including small source generator |
| NetSocketGenerator.CacheQueue | Simple In-Memory Key-Value Store with Subscribe Queue capability and clustering |

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
