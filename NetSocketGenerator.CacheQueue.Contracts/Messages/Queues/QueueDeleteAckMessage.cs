namespace NetSocketGenerator.CacheQueue.Contracts.Messages.Queues;

public sealed class QueueDeleteAckMessage : AckMessageBase
{
   public required bool IsFound { get; set; }
   
   public required bool IsDeleted { get; set; }
}