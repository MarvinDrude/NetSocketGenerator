using System.Text.Json;

namespace NetSocketGenerator.CacheQueue.Client.Processors.Queues;

[SocketProcessor(
   EventNamePattern = QueueEventNames.PublishConsumerAck,
   RegistrationGroups = ["Queue"],
   IncludeServer = false
)]
public sealed partial class ReceiveQueueConsumerAckProcessor
{
   public ReceiveQueueConsumerAckProcessor()
   {
   }

   public Task Execute(
      ITcpClient connection,
      [SocketPayload] QueuePublishConsumerAckMessage<JsonElement> message)
   {
      var client = connection.GetMetadata<CacheQueueClient>();
      client.AckContainer.TrySetResult(message.AckRequestId, message);
      
      return Task.CompletedTask;
   }
}