
using NetSocketGenerator.Tcp;
using NetSocketGenerator.Tcp.Frames;
using NetSocketGenerator.Tcp.Interfaces;


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
      OnConnected = (connection) =>
      {
         Console.WriteLine("[Server] Client Connected");
         return Task.CompletedTask;
      },
      OnDisconnected = (connection) =>
      {
         Console.WriteLine("[Server] Client Disconnected");
         return Task.CompletedTask;
      },
      OnFrameReceived = (connection, frame) =>
      {
         Interlocked.Increment(ref messagesReceived);
         //Console.WriteLine("[Server] Client sent frame");
         return Task.CompletedTask;
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
         OnConnected = (connection) =>
         {
            Console.WriteLine("[Client] Connected");
            _ = Task.Factory.StartNew(async () => await RunCommands(connection), TaskCreationOptions.LongRunning);
            return Task.CompletedTask;
         },
         OnDisconnected = (connection) =>
         {
            Console.WriteLine("[Client] Disconnected");
            return Task.CompletedTask;
         },
         OnFrameReceived = (connection, frame) =>
         {
            return Task.CompletedTask;
            //Console.WriteLine("[Client] received frame");
            //connection.Send("test", new byte[] { 2, 2, 2, 2, 2 });
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
   start = DateTimeOffset.UtcNow;
   Interlocked.Exchange(ref messagesReceived, 0);
}


static async Task RunCommands(ITcpConnection connection)
{
   while (true)
   {
      connection.Send("test", new byte[] { 2, 2, 2, 2, 2 });
      connection.Send("test", new byte[] { 2, 2, 2, 2, 2 });
      connection.Send("test", new byte[] { 2, 2, 2, 2, 2 });
      connection.Send("test", new byte[] { 2, 2, 2, 2, 2 });
      connection.Send("test", new byte[] { 2, 2, 2, 2, 2 });
   }
}