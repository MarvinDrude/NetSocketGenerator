
namespace NetSocketGenerator.CacheQueue.Server.Processors.Queues;

[SocketProcessor(
   EventNamePattern = QueueEventNames.Create,
   RegistrationGroups = ["Queue"],
   IncludeClient = false
)]
public sealed partial class CreateQueueProcessor
{
   private readonly ILogger<CreateQueueProcessor> _logger;

   public CreateQueueProcessor(
      ILogger<CreateQueueProcessor> logger)
   {
      _logger = logger;
   }
   
   public Task Execute(
      ITcpServerConnection connection,
      [SocketPayload] QueueCreateMessage message)
   {
      var queueServer = connection.CurrentServer.GetMetadata<CacheQueueServer>();
      using var scope = _logger.BeginScope(queueServer.NodeName);
      
      LogCreateQueue(message.QueueName, message.SubscribeImmediately);

      if (!queueServer.Options.IsClustered)
      {
         var queueDefinition = queueServer.QueueRegistry.CreateLocalQueue(message, queueServer);

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