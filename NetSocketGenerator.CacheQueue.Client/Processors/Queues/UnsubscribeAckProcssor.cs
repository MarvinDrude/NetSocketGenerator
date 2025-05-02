namespace NetSocketGenerator.CacheQueue.Client.Processors.Queues;

[SocketProcessor(
   EventNamePattern = QueueEventNames.Unsubscribe,
   RegistrationGroups = ["Queue"],
   IncludeServer = false
)]
public sealed partial class UnsubscribeAckProcssor
{
   public UnsubscribeAckProcssor()
   {
   }

   public Task Execute(
      ITcpClient connection,
      [SocketPayload] QueueUnsubscribeAckMessage message)
   {
      var client = connection.GetMetadata<CacheQueueClient>();
      client.AckContainer.TrySetResult(message.AckRequestId, message);
      
      return Task.CompletedTask;
   }
}