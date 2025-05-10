using NetSocketGenerator.CacheQueue.Configuration.Cluster;

namespace NetSocketGenerator.CacheQueue.Configuration.Server;

public sealed class CacheQueueServerOptions
{
   public required string Address { get; init; }
   
   public required int Port { get; init; }
   
   public required IServiceProvider ServiceProvider { get; init; }
   
   public int BucketCount { get; init; } = Math.Min(24, Environment.ProcessorCount);
   
   public bool IsClustered => ClusterOptions.Nodes.Count > 1;
   
   public CacheQueueClusterOptions ClusterOptions { get; init; } = new CacheQueueClusterOptions()
   {
      CurrentNodeName = "local",
      Secret = "secret-text-key",
      Nodes = []
   };
}