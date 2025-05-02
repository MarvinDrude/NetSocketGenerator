namespace NetSocketGenerator.CacheQueue.Models.Common;

public sealed class ServerClientProperties
{
   public ConcurrentDictionary<string, ServerQueueSubscription> QueueSubscriptions { get; } = [];
}