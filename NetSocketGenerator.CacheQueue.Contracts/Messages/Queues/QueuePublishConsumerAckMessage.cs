namespace NetSocketGenerator.CacheQueue.Contracts.Messages.Queues;

public class QueuePublishConsumerAckMessage<T> : AckMessageBase
{
   public required bool AwaitsAck { get; set; }
   
   public required T Contents { get; set; }
   
   public required string QueueName { get; set; }
}