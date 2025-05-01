
namespace NetSocketGenerator.CacheQueue.Server;

public sealed class CacheQueueServer
{
   private readonly CacheQueueServerOptions _options;
   private readonly TcpServer _server;
   
   public CacheQueueServer(CacheQueueServerOptions options)
   {
      _options = options;
      _server = new TcpServer(new TcpServerOptions()
      {
         Address = _options.Address,
         Port = _options.Port,
         ServiceProvider = _options.ServiceProvider,
         Serializer = new TcpDynamicJsonSerializer(),
         Events = new TcpEventCallbacks()
         {
            OnConnected = OnClientConnected,
            OnDisconnected = OnClientDisconnected,
         }
      })
      {
         MetadataObjectReference = this
      };
   }

   public void Start()
   {
      _server.Start();
   }

   public async Task Stop()
   {
      await _server.Stop();
   }
   
   private async Task OnClientConnected(ITcpConnection connection)
   {
      
   }

   private async Task OnClientDisconnected(ITcpConnection connection)
   {
      
   }
}