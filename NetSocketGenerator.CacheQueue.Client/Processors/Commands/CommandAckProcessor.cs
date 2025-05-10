
namespace NetSocketGenerator.CacheQueue.Client.Processors.Commands;

[SocketProcessor(
   EventNamePattern = EventNames.CommandAck,
   RegistrationGroups = ["Commands"],
   IncludeServer = false
)]
public sealed partial class CommandAckProcessor
{
   public CommandAckProcessor()
   {
   }

   public Task Execute(
      ITcpClient connection,
      [SocketPayload] AckMessageBase message)
   {
      var client = connection.GetMetadata<CacheQueueClient>();
      client.AckContainer.TrySetResult(message.AckRequestId, (object)message);
      
      return Task.CompletedTask;
   }
}