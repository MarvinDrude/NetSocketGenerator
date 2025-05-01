namespace NetSocketGenerator.CacheQueue.Contracts.Messages.Queues;

public sealed class QueueUnsubscribeMessage : MessageBase
{
   public required string QueueName { get; set; }
}