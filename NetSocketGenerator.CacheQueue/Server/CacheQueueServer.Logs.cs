namespace NetSocketGenerator.CacheQueue.Server;

public sealed partial class CacheQueueServer
{
   [LoggerMessage(
      eventId: 1,
      level: LogLevel.Information,
      message: "Starting CacheQueueServer '{Name}' on {Address}:{Port}, Cluster Mode: {IsClustered}"
   )]
   private partial void LogStart(string name, string address, int port, bool isClustered);
}