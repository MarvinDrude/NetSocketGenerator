namespace NetSocketGenerator.CacheQueue.Contracts.Messages.Queues;

public class QueuePublishConsumerAckMessage<T> : AckMessageBase
{
   public required T Contents { get; set; }
}