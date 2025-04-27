
using NetSocketGenerator.Tcp;

var server = new TcpServer(new TcpServerOptions()
{
   Address = "127.0.0.1",
   Port = 12234,
   Events = new TcpEventCallbacks()
   {
      OnConnected = async (connection) =>
      {
         
      },
   }
});


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