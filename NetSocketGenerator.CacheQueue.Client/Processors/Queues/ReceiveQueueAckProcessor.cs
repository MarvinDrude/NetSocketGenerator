
namespace NetSocketGenerator.CacheQueue.Client.Processors.Queues;

[SocketProcessor(
   EventNamePattern = QueueEventNames.PublishAck,
   RegistrationGroups = ["Queue"],
   IncludeServer = false
)]
public sealed partial class ReceiveQueueAckProcessor
{
   public ReceiveQueueAckProcessor()
   {
   }

   public Task Execute(
      ITcpClient connection,
      [SocketPayload] QueuePublishAckMessage message)
   {
      var client = connection.GetMetadata<CacheQueueClient>();
      client.AckContainer.TrySetResult(message.AckRequestId, message);
      
      return Task.CompletedTask;
   }
}