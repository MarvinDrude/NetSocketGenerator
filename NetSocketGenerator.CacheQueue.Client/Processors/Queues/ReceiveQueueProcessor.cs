using System.Text.Json;

namespace NetSocketGenerator.CacheQueue.Client.Processors.Queues;

[SocketProcessor(
   EventNamePattern = QueueEventNames.Publish,
   RegistrationGroups = ["Queue"],
   IncludeServer = false
)]
public sealed partial class ReceiveQueueProcessor
{
   public ReceiveQueueProcessor()
   {
   }

   public async Task Execute(
      ITcpClient connection,
      [SocketPayload] QueuePublishMessage<JsonElement> message)
   {
      var client = connection.GetMetadata<CacheQueueClient>();

      if (client.Queue.Handlers.TryGetValue(message.QueueName, out var handlerContainer))
      {
         await handlerContainer.FireQueueMessage(client, message);
      }
   }
}