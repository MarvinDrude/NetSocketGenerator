using System.Text.Json;

namespace NetSocketGenerator.CacheQueue.Server.Processors.Queues;

[SocketProcessor(
   EventNamePattern = QueueEventNames.Publish,
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
   
   public async Task Execute(
      ITcpServerConnection connection,
      [SocketPayload] QueuePublishMessage<JsonElement> message)
   {
      var queueServer = connection.CurrentServer.GetMetadata<CacheQueueServer>();
      using var scope = _logger.BeginScope(queueServer.NodeName);
      
      if (!queueServer.Options.IsClustered)
      {
         var isPublished = false;
         if (queueServer.QueueRegistry.GetLocalQueue(message.QueueName) is { } queue)
         {
            await queue.PublishMessage(connection, message);
            isPublished = true;
         }
         
         if (message is { ConsumerAck: false, AwaitsAck: true })
         {
            connection.Send(QueueEventNames.Publish, 
               message.CreateAckMessage(new QueuePublishAckMessage()
               {
                  IsPublished = isPublished
               }));
         }
      }
      
   }
}