namespace NetSocketGenerator.CacheQueue.Server.Processors.Queues;

public sealed partial class CreateQueueProcessor
{
   [LoggerMessage(
      eventId: 1,
      level: LogLevel.Debug,
      message: "Creating Queue '{QueueName}', subscribe immediately: {SubscribeImmediately}."
   )]
   private partial void LogCreateQueue(string queueName, bool subscribeImmediately);
}