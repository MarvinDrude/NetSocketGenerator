
namespace NetSocketGenerator.CacheQueue.Server;

public sealed partial class CacheQueueServer : IAsyncDisposable
{
   internal CacheQueueServerOptions Options { get; }
   internal CacheQueueRegistry QueueRegistry { get; } = new();
   
   private readonly TcpServer _server;

   private readonly ILogger<CacheQueueServer> _logger;
   
   public CacheQueueServer(
      ILogger<CacheQueueServer> logger,
      CacheQueueServerOptions options)
   {
      _logger = logger;
      
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

      _server.UseSocketServerQueueProcessors();
   }

   public void Start()
   {
      _server.Start();
      LogStart(Options.ClusterOptions.CurrentNodeName, Options.Address, Options.Port, Options.IsClustered);
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

   public async ValueTask DisposeAsync()
   {
      await Stop();
   }
}