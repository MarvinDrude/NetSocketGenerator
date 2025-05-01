namespace NetSocketGenerator.CacheQueue.Contracts.Messages.Queues;

public sealed class QueueDeleteMessage : MessageBase
{
   public required string QueueName { get; set; }   
}