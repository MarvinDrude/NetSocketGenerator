namespace NetSocketGenerator.CacheQueue.Contracts.Messages.Queues;

public sealed class QueueSubscribeMessage
{
   public required string QueueName { get; set; }
}