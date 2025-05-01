
namespace NetSocketGenerator.CacheQueue.Server;

public sealed class CacheQueueRegistry
{
   private readonly ConcurrentDictionary<string, ServerQueueDefinition> _localQueues = [];

   public ServerQueueDefinition CreateLocalQueue(QueueCreateMessage createParameters)
   {
      return _localQueues.GetOrAdd(createParameters.QueueName, _ => new ServerQueueDefinition()
      {
         Name = createParameters.QueueName
      });
   }
}