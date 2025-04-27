
using NetSocketGenerator.Tcp;
using NetSocketGenerator.Tcp.Frames;


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

long messagesReceived = 0;
var start = DateTimeOffset.UtcNow;

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
         //Console.WriteLine("[Server] Client sent frame");
         connection.Send("test", "test1");
         Interlocked.Increment(ref messagesReceived);
      }
   }
});
server.Start();

for (var e = 0; e < 1; e++)
{
   var client = new TcpClient(new TcpClientOptions()
   {
      Address = "127.0.0.1",
      Port = 12234,
      ReconnectInterval = TimeSpan.FromSeconds(100),
      Events = new TcpEventCallbacks()
      {
         OnConnected = async (connection) =>
         {
            Console.WriteLine("[Client] Connected");
            connection.Send("test", new byte[] { 2, 2, 2, 2, 2 });
         },
         OnDisconnected = async (connection) =>
         {
            Console.WriteLine("[Client] Disconnected");
         },
         OnFrameReceived = async (connection, frame) =>
         {
            //Console.WriteLine("[Client] received frame");
            connection.Send("test", new byte[] { 2, 2, 2, 2, 2 });
         }
      }
   });
   client.Connect();
}

while (true)
{
   await Task.Delay(1_000);
   var elapsed = DateTimeOffset.UtcNow - start;
   Console.WriteLine($"Messages received per second: {(messagesReceived / elapsed.TotalSeconds)}");
}
