namespace NetSocketGenerator.CacheQueue.Models.Queues;

public class ServerQueueSubscription
{
   public required string QueueName { get; init; }
   
   public required Guid ClientId { get; init; }
   
   public required ServerQueueDefinition Definition { get; init; }
}

public sealed class ServerLocalQueueSubscription
   : ServerQueueSubscription
{
   
}

public sealed class ServerClusterQueueSubscription
   : ServerQueueSubscription
{
   public required string NodeName { get; init; }
}