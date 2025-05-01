namespace NetSocketGenerator.CacheQueue.Contracts.Messages.Queues;

public sealed class QueueCreateMessage : MessageBase
{
   public required string QueueName { get; set; }
   
   public required bool SubscribeImmediately { get; set; }
}