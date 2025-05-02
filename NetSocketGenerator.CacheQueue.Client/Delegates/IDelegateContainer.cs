using System.Text.Json;

namespace NetSocketGenerator.CacheQueue.Client.Delegates;

public interface IDelegateContainer
{
   public Task FireQueueMessage(
      CacheQueueClient client,
      QueuePublishMessage<JsonElement> publishMessage);
}