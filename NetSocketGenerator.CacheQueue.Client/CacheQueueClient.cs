using TcpClient = NetSocketGenerator.Tcp.TcpClient;

namespace NetSocketGenerator.CacheQueue.Client;

public sealed class CacheQueueClient : IAsyncDisposable
{
   internal CacheQueueClientOptions Options { get; }
   
   private readonly TcpClient _client;
   internal readonly AckContainer AckContainer = new();
   
   public CacheQueueClient(
      CacheQueueClientOptions options)
   {
      Options = options;
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
      })
      {
         MetadataObjectReference = this
      };
      
      _client.UseSocketClientQueueProcessors();
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

   public async Task<bool> QueueCreate(string queueName)
   {
      var message = new QueueCreateMessage()
      {
         QueueName = queueName,
         SubscribeImmediately = false
      };
      
      return await QueueCreate(message);
   }
   
   public async Task<bool> QueueCreate(QueueCreateMessage message)
   {
      message.AwaitsAck = true;
      _client.Send(QueueEventNames.Create, message);
      
      var result = await AckContainer
         .Enqueue<QueueCreateAckMessage>(message.RequestId, Options.ServerAckTimeout);

      return result is not null;
   }

   public void QueueCreateNoAck(string queueName)
   {
      var message = new QueueCreateMessage()
      {
         QueueName = queueName,
         SubscribeImmediately = false
      };
      QueueCreateNoAck(message);
   }
   
   public void QueueCreateNoAck(QueueCreateMessage message)
   {
      message.AwaitsAck = false;
      _client.Send(QueueEventNames.Create, message);
   }
   
   public async ValueTask DisposeAsync()
   {
      await _client.Disconnect();
      AckContainer.Dispose();
   }
}