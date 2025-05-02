using NetSocketGenerator.CacheQueue.Contracts.Constants;

namespace NetSocketGenerator.CacheQueue.Server.Processors.Queues;

[SocketProcessor(
   EventNamePattern = QueueEventNames.Delete,
   RegistrationGroups = ["Queue"],
   IncludeClient = false
)]
public sealed partial class DeleteQueueProcessor
{
   private readonly ILogger<DeleteQueueProcessor> _logger;

   public DeleteQueueProcessor(
      ILogger<DeleteQueueProcessor> logger)
   {
      _logger = logger;
   }
   
   public Task Execute(
      ITcpServerConnection connection,
      [SocketPayload] QueueCreateMessage message)
   {
      var queueServer = connection.CurrentServer.GetMetadata<CacheQueueServer>();

      if (!queueServer.Options.IsClustered)
      {
         var queueDefinition = queueServer.QueueRegistry.CreateLocalQueue(message);

         if (message.SubscribeImmediately)
         {
            queueDefinition.AddLocalSubscription(connection.Id);
         }

         if (message.AwaitsAck)
         {
            connection.Send(QueueEventNames.Create, 
               message.CreateAckMessage(new QueueCreateAckMessage()));
         }
      }
         
      return Task.CompletedTask;
   }
}