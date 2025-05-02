namespace NetSocketGenerator.CacheQueue.Contracts.Messages.Queues;

public sealed class QueuePublishMessage<T> : MessageBase
{
   public required string QueueName { get; set; }
   
   public required bool ConsumerAck { get; set; }
   
   public required T Contents { get; set; }
}
