
namespace NetSocketGenerator.CacheQueue.Server;

public sealed partial class CacheQueueServer : IAsyncDisposable
{
   public string NodeName => Options.ClusterOptions.CurrentNodeName;
   
   internal CacheQueueServerOptions Options { get; }
   internal CacheQueueRegistry QueueRegistry { get; } = new();
   
   internal ConcurrentDictionary<Guid, ServerClientProperties> Clients { get; } = [];
   
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
      Clients.Clear();
   }
   
   private Task OnClientConnected(ITcpConnection connection)
   {
      Clients[connection.Id] = new ServerClientProperties();
      return Task.CompletedTask;
   }

   private Task OnClientDisconnected(ITcpConnection connection)
   {
      if (Clients.TryRemove(connection.Id, out var properties))
      {
         foreach (var subscription in properties.QueueSubscriptions.Values)
         {
            subscription.Definition.RemoveLocalSubscription(connection.Id);
         }
      }
      return Task.CompletedTask;
   }

   internal ITcpServerConnection? GetConnection(Guid connectionId)
   {
      return _server.GetConnection(connectionId);
   }
   
   public async ValueTask DisposeAsync()
   {
      await Stop();
   }
}