namespace NetSocketGenerator.CacheQueue.Client.Processors.Queues;

[SocketProcessor(
   EventNamePattern = QueueEventNames.Create,
   RegistrationGroups = ["Queue"],
   IncludeServer = false
)]
public sealed partial class CreateQueueAckProcessor
{
   public CreateQueueAckProcessor()
   {
   }

   public Task Execute(
      ITcpClient connection,
      [SocketPayload] QueueCreateAckMessage message)
   {
      var client = connection.GetMetadata<CacheQueueClient>();
      client.AckContainer.TrySetResult(message.AckRequestId, message);
      
      return Task.CompletedTask;
   }
}