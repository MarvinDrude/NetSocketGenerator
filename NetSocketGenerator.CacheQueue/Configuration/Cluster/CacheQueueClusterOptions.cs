namespace NetSocketGenerator.CacheQueue.Configuration.Cluster;

public sealed class CacheQueueClusterOptions
{
   public required string CurrentNodeName { get; init; }
   
   public required string Secret { get; init; }
   
   public required List<CacheQueueClusterNodeOptions> Nodes { get; init; }
}