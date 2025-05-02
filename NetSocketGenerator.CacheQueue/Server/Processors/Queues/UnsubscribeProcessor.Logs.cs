namespace NetSocketGenerator.CacheQueue.Server.Processors.Queues;

public sealed partial class UnsubscribeProcessor
{
   [LoggerMessage(
      eventId: 1,
      level: LogLevel.Debug,
      message: "Unsubscribing queue '{QueueName}'."
   )]
   private partial void LogQueueUnsubscribe(string queueName);
}