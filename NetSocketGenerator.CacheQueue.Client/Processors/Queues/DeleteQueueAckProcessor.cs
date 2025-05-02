namespace NetSocketGenerator.CacheQueue.Client.Processors.Queues;

[SocketProcessor(
   EventNamePattern = QueueEventNames.Delete,
   RegistrationGroups = ["Queue"],
   IncludeServer = false
)]
public sealed partial class DeleteQueueAckProcessor
{
   public DeleteQueueAckProcessor()
   {
   }

   public Task Execute(
      ITcpClient connection,
      [SocketPayload] QueueDeleteAckMessage message)
   {
      var client = connection.GetMetadata<CacheQueueClient>();
      client.AckContainer.TrySetResult(message.AckRequestId, message);
      
      return Task.CompletedTask;
   }
}