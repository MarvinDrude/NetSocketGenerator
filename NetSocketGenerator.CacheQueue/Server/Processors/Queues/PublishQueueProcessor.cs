namespace NetSocketGenerator.CacheQueue.Server.Processors.Queues;

[SocketProcessor(
   EventNamePattern = QueueEventNames.Delete,
   RegistrationGroups = ["Queue"],
   IncludeClient = false
)]
public sealed partial class PublishQueueProcessor
{
   private readonly ILogger<PublishQueueProcessor> _logger;

   public PublishQueueProcessor(
      ILogger<PublishQueueProcessor> logger)
   {
      _logger = logger;
   }
   
   public Task Execute(
      ITcpServerConnection connection,
      [SocketPayload] QueuePublishMessage<> message)
   {
      var queueServer = connection.CurrentServer.GetMetadata<CacheQueueServer>();
      using var scope = _logger.BeginScope(queueServer.NodeName);
      
      if (!queueServer.Options.IsClustered)
      {
         var definition = queueServer.QueueRegistry.GetLocalQueue(message.QueueName);
         
         
      }
         
      return Task.CompletedTask;
   }
}