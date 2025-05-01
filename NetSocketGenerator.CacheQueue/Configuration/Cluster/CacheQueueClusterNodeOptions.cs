namespace NetSocketGenerator.CacheQueue.Configuration.Cluster;

public sealed class CacheQueueClusterNodeOptions
{
   public required string NodeName { get; init; }
   
   public required string Address { get; init; }
   
   public required int Port { get; init; }
}