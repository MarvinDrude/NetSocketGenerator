
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
      [SocketPayload] QueueDeleteMessage message)
   {
      var queueServer = connection.CurrentServer.GetMetadata<CacheQueueServer>();
      using var scope = _logger.BeginScope(queueServer.NodeName);
      
      LogQueueDelete(message.QueueName);

      if (!queueServer.Options.IsClustered)
      {
         var definition = queueServer.QueueRegistry.DeleteLocalQueue(message.QueueName);

         if (definition is not null)
         {
            // maybe notify
         }
         
         if (message.AwaitsAck)
         {
            connection.Send(QueueEventNames.Create, 
               message.CreateAckMessage(new QueueDeleteAckMessage()
               {
                  IsDeleted = true,
                  IsFound = definition is not null
               }));
         }
      }
         
      return Task.CompletedTask;
   }
}