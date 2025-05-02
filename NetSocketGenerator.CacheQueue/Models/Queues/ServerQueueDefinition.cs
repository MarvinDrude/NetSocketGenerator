
namespace NetSocketGenerator.CacheQueue.Models.Queues;

public sealed class ServerQueueDefinition
{
   public required string Name { get; init; }

   public required CacheQueueServer Owner { get; init; }
   
   public ConcurrentDictionary<Guid, ServerQueueSubscription> Subscriptions { get; } = [];
   
   public void AddLocalSubscription(Guid connectionId)
   {
      Subscriptions[connectionId] = new ServerLocalQueueSubscription()
      {
         QueueName = Name,
         ClientId = connectionId
      };
   }
   
   public bool RemoveLocalSubscription(Guid connectionId)
   {
      return Subscriptions.TryRemove(connectionId, out _);
   }
}