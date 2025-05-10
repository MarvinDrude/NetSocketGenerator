namespace NetSocketGenerator.CacheQueue.Bucketing;

public sealed partial class BucketExecutor
{
   [LoggerMessage(
      eventId: 1,
      level: LogLevel.Error,
      message: "Error handling command."
   )]
   private partial void LogFatalCommandError(Exception error);
   
   [LoggerMessage(
      eventId: 2,
      level: LogLevel.Debug,
      message: "Running command {Command}."
   )]
   private partial void LogRunningCommand(BaseCommand command);
}