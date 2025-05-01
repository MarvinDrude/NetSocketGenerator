namespace NetSocketGenerator.CacheQueue.Contracts.Messages.Queues;

public sealed class QueueUnsubscribeAckMessage : AckMessageBase
{
   public required bool IsFound { get; set; }
   
   public required bool IsUnsubscribed { get; set; }
}