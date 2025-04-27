
using NetSocketGenerator.Tcp;
using NetSocketGenerator.Tcp.Frames;
using NetSocketGenerator.Tcp.Interfaces;


public sealed class PingHandler
{
   public PingHandler()
   {
      
   }

   public async Task Execute()
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