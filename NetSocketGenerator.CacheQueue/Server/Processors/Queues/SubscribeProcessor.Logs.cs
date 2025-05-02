namespace NetSocketGenerator.CacheQueue.Server.Processors.Queues;

public sealed partial class SubscribeProcessor
{
   [LoggerMessage(
      eventId: 1,
      level: LogLevel.Debug,
      message: "Subscribing queue '{QueueName}'."
   )]
   private partial void LogQueueSubscribe(string queueName);
}