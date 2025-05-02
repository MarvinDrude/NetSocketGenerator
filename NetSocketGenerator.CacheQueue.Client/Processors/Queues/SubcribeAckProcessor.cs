namespace NetSocketGenerator.CacheQueue.Client.Processors.Queues;

[SocketProcessor(
   EventNamePattern = QueueEventNames.Subscribe,
   RegistrationGroups = ["Queue"],
   IncludeServer = false
)]
public sealed partial class SubscribeAckProcessor
{
   public SubscribeAckProcessor()
   {
   }

   public Task Execute(
      ITcpClient connection,
      [SocketPayload] QueueSubscribeAckMessage message)
   {
      var client = connection.GetMetadata<CacheQueueClient>();
      client.AckContainer.TrySetResult(message.AckRequestId, message);
      
      return Task.CompletedTask;
   }
}