namespace NetSocketGenerator.CacheQueue.Contracts.Messages.Queues;

public sealed class QueuePublishAckMessage : AckMessageBase
{
   public required bool IsPublished { get; set; }
}