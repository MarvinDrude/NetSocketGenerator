
using TcpClient = NetSocketGenerator.Tcp.TcpClient;

namespace NetSocketGenerator.CacheQueue.Client;

public sealed class CacheQueueClient : IAsyncDisposable
{
   public QueueModule Queue { get; }
   public StringModule Strings { get; }
   
   internal CacheQueueClientOptions Options { get; }
   
   internal readonly TcpClient Tcp;
   internal readonly AckContainer AckContainer = new();
   
   public CacheQueueClient(
      CacheQueueClientOptions options)
   {
      Options = options;
      Tcp = new TcpClient(new TcpClientOptions()
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
      
      Tcp.UseSocketClientQueueProcessors();

      Queue = new QueueModule(this);
      Strings = new StringModule(this);
   }
   
   public void Connect()
   {
      Tcp.Connect();
   }

   public async Task Disconnect()
   {
      await Tcp.Disconnect();
   }
   
   private async Task OnConnected(ITcpConnection connection)
   {
      
   }

   private async Task OnDisconnected(ITcpConnection connection)
   {
      
   }

   
   public async ValueTask DisposeAsync()
   {
      await Tcp.Disconnect();
      AckContainer.Dispose();
   }
}