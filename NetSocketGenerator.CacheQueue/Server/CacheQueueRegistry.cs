
namespace NetSocketGenerator.CacheQueue.Server;

public sealed class CacheQueueRegistry
{
   private readonly ConcurrentDictionary<string, ServerQueueDefinition> _localQueues = [];

   public ServerQueueDefinition CreateLocalQueue(
      QueueCreateMessage createParameters, CacheQueueServer server)
   {
      return _localQueues.GetOrAdd(createParameters.QueueName, _ => new ServerQueueDefinition()
      {
         Name = createParameters.QueueName,
         Owner = server
      });
   }

   public ServerQueueDefinition? DeleteLocalQueue(string queueName)
   {
      return _localQueues.TryRemove(queueName, out var queue) 
         ? queue : null;
   }

   public ServerQueueDefinition? GetLocalQueue(string queueName)
   {
      return _localQueues.GetValueOrDefault(queueName);
   }
}