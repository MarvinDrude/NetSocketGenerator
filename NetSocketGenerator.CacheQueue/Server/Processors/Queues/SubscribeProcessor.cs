
namespace NetSocketGenerator.CacheQueue.Server.Processors.Queues;

[SocketProcessor(
   EventNamePattern = QueueEventNames.Subscribe,
   RegistrationGroups = ["Queue"],
   IncludeClient = false
)]
public sealed partial class SubscribeProcessor
{
   public SubscribeProcessor()
   {
      
   }
   
   public async Task Execute(
      ITcpServerConnection connection,
      [SocketPayload] QueueSubscribeMessage message)
   {
      var queueServer = connection.CurrentServer.GetMetadata<CacheQueueServer>();

      if (!queueServer.Options.IsClustered)
      {
         
         return;
      }
   }
   
   
}