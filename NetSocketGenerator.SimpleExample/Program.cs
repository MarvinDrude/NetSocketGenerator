﻿
using Microsoft.Extensions.DependencyInjection;
using NetSocketGenerator.Attributes;
using NetSocketGenerator.Extensions;
using NetSocketGenerator.Tcp;
using NetSocketGenerator.Tcp.Frames;
using NetSocketGenerator.Tcp.Interfaces;

Console.WriteLine("aaa");

var collection = new ServiceCollection();
collection.AddSocketClientProcessors();
collection.AddSocketServerProcessors();

var serviceProvider = collection.BuildServiceProvider();

var server = new TcpServer(new TcpServerOptions()
{
   Address = "127.0.0.1",
   Port = 34533,
   ServiceProvider = serviceProvider,
});
server.UseSocketServerProcessors();
server.Start();

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

while (true)
{
   await Task.Delay(1_000);
}

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

// var server = new TcpServer(new TcpServerOptions()
// {
//    Address = "127.0.0.1",
//    Port = 12234,
//    Events = new TcpEventCallbacks()
//    {
//       OnConnected = (connection) =>
//       {
//          Console.WriteLine("[Server] Client Connected");
//          return Task.CompletedTask;
//       },
//       OnDisconnected = (connection) =>
//       {
//          Console.WriteLine("[Server] Client Disconnected");
//          return Task.CompletedTask;
//       },
//    }
// });
//
// server.AddHandler("test-message", (connection, key, payload) =>
// {
//    connection.Send("test-message", payload);
//    Console.WriteLine("[Server] Client sent message");
//    return Task.CompletedTask;
// });
//
// server.Start();
//
// for (var e = 0; e < 1; e++)
// {
//    var client = new TcpClient(new TcpClientOptions()
//    {
//       Address = "127.0.0.1",
//       Port = 12234,
//       ReconnectInterval = TimeSpan.FromSeconds(100),
//       Events = new TcpEventCallbacks()
//       {
//          OnConnected = (connection) =>
//          {
//             Console.WriteLine("[Client] Connected");
//             connection.Send("test-message", "adsadsa sa dsa");
//             return Task.CompletedTask;
//          },
//          OnDisconnected = (connection) =>
//          {
//             Console.WriteLine("[Client] Disconnected");
//             return Task.CompletedTask;
//          },
//       }
//    });
//    
//    client.AddHandler("test-message", async (connection, key, payload) =>
//    {
//       connection.Send("test-message", payload);
//       await Task.Delay(1000);
//       Console.WriteLine("[Client] Server sent message");
//    });
//    
//    client.Connect();
// }

// while (true)
// {
//    await Task.Delay(1_000);
// }