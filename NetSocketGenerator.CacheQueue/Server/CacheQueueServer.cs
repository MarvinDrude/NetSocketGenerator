
namespace NetSocketGenerator.CacheQueue.Server;

public sealed class CacheQueueServer
{
   internal CacheQueueServerOptions Options { get; }
   internal CacheQueueRegistry QueueRegistry { get; } = new();
   
   private readonly TcpServer _server;
   
   public CacheQueueServer(CacheQueueServerOptions options)
   {
      Options = options;
      _server = new TcpServer(new TcpServerOptions()
      {
         Address = Options.Address,
         Port = Options.Port,
         ServiceProvider = Options.ServiceProvider,
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