
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
      if (!_localQueues.TryRemove(queueName, out var queue))
      {
         return null;
      }

      queue.Dispose();
      return queue;
   }

   public ServerQueueDefinition? GetLocalQueue(string queueName)
   {
      return _localQueues.GetValueOrDefault(queueName);
   }
}