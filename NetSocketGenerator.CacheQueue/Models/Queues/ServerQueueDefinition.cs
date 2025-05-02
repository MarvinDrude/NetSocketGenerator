
using System.Text.Json;

namespace NetSocketGenerator.CacheQueue.Models.Queues;

public sealed class ServerQueueDefinition : IDisposable
{
   public required string Name { get; init; }

   public required CacheQueueServer Owner { get; init; }
   
   public ConcurrentDictionary<Guid, ServerQueueSubscription> Subscriptions { get; } = [];
   
   public AckContainer AckContainer { get; } = new();
   
   public void AddLocalSubscription(Guid connectionId)
   {
      var subscription = Subscriptions[connectionId] = new ServerLocalQueueSubscription()
      {
         QueueName = Name,
         ClientId = connectionId,
         Definition = this
      };
      
      if (Owner.Clients.TryGetValue(connectionId, out var client))
      {
         client.QueueSubscriptions[Name] = subscription;
      }
   }
   
   public bool RemoveLocalSubscription(Guid connectionId)
   {
      if (Owner.Clients.TryGetValue(connectionId, out var client))
      {
         client.QueueSubscriptions.TryRemove(Name, out _);
      }
      
      return Subscriptions.TryRemove(connectionId, out _);
   }
   
   public async Task PublishMessage(ITcpServerConnection conn, QueuePublishMessage<JsonElement> message)
   {
      Task<QueuePublishConsumerAckMessage<JsonElement>?>? task = null;
      
      if (message.ConsumerAck)
      {
         task = AckContainer.Enqueue<QueuePublishConsumerAckMessage<JsonElement>?>(message.RequestId, message.ConsumerAckTimeout);
      }
      
      foreach (var subscription in Subscriptions.Values)
      {
         _ = subscription switch
         {
            ServerLocalQueueSubscription localSubscription => PublishLocalMessage(localSubscription, message),
            ServerClusterQueueSubscription => throw new NotImplementedException(),
            _ => throw new NotImplementedException()
         };
      }

      if (task is not null
          && await task is { } ackMessage)
      {
         conn.Send(QueueEventNames.PublishAck, ackMessage);
      }
   }
   
   private bool PublishLocalMessage(
      ServerLocalQueueSubscription localSubscription, 
      QueuePublishMessage<JsonElement> message)
   {
      if (Owner.GetConnection(localSubscription.ClientId) is not { } connection)
      {
         return false;
      }

      connection.Send(QueueEventNames.Publish, message);
      return true;
   }

   public void Dispose()
   {
      AckContainer.Dispose();
   }
}