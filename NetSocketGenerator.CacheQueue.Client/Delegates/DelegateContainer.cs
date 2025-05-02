using System.Text.Json;

namespace NetSocketGenerator.CacheQueue.Client.Delegates;

public sealed class DelegateContainer<T> : IDelegateContainer
{
   private readonly ConcurrentBag<QueuePublishDelegate<T>> _delegates = [];

   public async Task FireQueueMessage(
      CacheQueueClient client,
      QueuePublishMessage<JsonElement> publishMessage)
   {
      var message = publishMessage.Contents.Deserialize<T>(TcpDynamicJsonSerializer.JsonOptions);

      if (message is null)
      {
         return;
      }
      
      var context = new QueuePublishContext<T>()
      {
         Client = client,
         QueueName = publishMessage.QueueName,
         Message = message,
         RawMessage = publishMessage,
      };
      
      foreach (var delegateToRun in _delegates)
      {
         await delegateToRun(context);
      }
   }
   
   public void Add(QueuePublishDelegate<T> delegateToAdd)
   {
      _delegates.Add(delegateToAdd);
   }
}