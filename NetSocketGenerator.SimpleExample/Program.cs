
using NetSocketGenerator.Tcp;
using NetSocketGenerator.Tcp.Frames;

var server = new TcpServer(new TcpServerOptions()
{
   Address = "127.0.0.1",
   Port = 12234,
   Events = new TcpEventCallbacks()
   {
      OnConnected = async (connection) =>
      {
         Console.WriteLine("[Server] Client Connected");
      },
      OnDisconnected = async (connection) =>
      {
         Console.WriteLine("[Server] Client Disconnected");
      },
      OnFrameReceived = async (connection, frame) =>
      {
         Console.WriteLine("[Server] Client sent frame");
      }
   }
});
server.Start();

var client = new TcpClient(new TcpClientOptions()
{
   Address = "127.0.0.1",
   Port = 12234,
   Events = new TcpEventCallbacks()
   {
      OnConnected = async (connection) =>
      {
         await Task.Delay(1000);
         Console.WriteLine("[Client] Connected");
         connection.SendFrame<TcpFrame>("test", "test1");
      },
      OnDisconnected = async (connection) =>
      {
         Console.WriteLine("[Client] Disconnected");
      },
      OnFrameReceived = async (connection, frame) =>
      {
         Console.WriteLine("[Client] received frame");
      }
   }
});
client.Connect();

while (true)
{
   await Task.Delay(20_000);
}

// public sealed class PingHandler
// {
//    public PingHandler()
//    {
//       
//    }
//
//    public async Task Execute()
//    {
//       
//    }
// }