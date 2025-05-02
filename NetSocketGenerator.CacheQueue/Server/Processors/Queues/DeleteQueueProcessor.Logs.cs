namespace NetSocketGenerator.CacheQueue.Server.Processors.Queues;

public sealed partial class DeleteQueueProcessor
{
   [LoggerMessage(
      eventId: 1,
      level: LogLevel.Debug,
      message: "Deleting queue '{QueueName}'."
   )]
   private partial void LogQueueDelete(string queueName);
}