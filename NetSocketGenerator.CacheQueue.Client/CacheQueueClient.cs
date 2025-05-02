using NetSocketGenerator.CacheQueue.Contracts.Constants;
using TcpClient = NetSocketGenerator.Tcp.TcpClient;

namespace NetSocketGenerator.CacheQueue.Client;

public sealed class CacheQueueClient : IAsyncDisposable
{
   private readonly TcpClient _client;

   public CacheQueueClient(
      CacheQueueClientOptions options)
   {
      _client = new TcpClient(new TcpClientOptions()
      {
         Address = options.Address,
         Port = options.Port,
         ServiceProvider = options.ServiceProvider,
         Serializer = new TcpDynamicJsonSerializer(),
         ReconnectInterval = TimeSpan.FromSeconds(5),
         Events = new TcpEventCallbacks()
         {
            OnConnected = OnConnected,
            OnDisconnected = OnDisconnected,
         }
      });
   }

   public async Task QueueCreate(QueueCreateMessage message)
   {
      message.AwaitsAck = true;
      
   }
   
   public void QueueCreateNoAck(QueueCreateMessage message)
   {
      message.AwaitsAck = false;
      _client.Send(QueueEventNames.Create, message);
   }

   public void Connect()
   {
      _client.Connect();
   }

   public async Task Disconnect()
   {
      await _client.Disconnect();
   }
   
   private async Task OnConnected(ITcpConnection connection)
   {
      
   }

   private async Task OnDisconnected(ITcpConnection connection)
   {
      
   }
   
   public async ValueTask DisposeAsync()
   {
      await _client.Disconnect();
   }
}