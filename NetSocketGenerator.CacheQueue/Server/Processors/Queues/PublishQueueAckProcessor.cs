using System.Text.Json;

namespace NetSocketGenerator.CacheQueue.Server.Processors.Queues;

[SocketProcessor(
   EventNamePattern = QueueEventNames.PublishAck,
   RegistrationGroups = ["Queue"],
   IncludeClient = false
)]
public sealed partial class PublishQueueAckProcessor
{
   private readonly ILogger<PublishQueueAckProcessor> _logger;

   public PublishQueueAckProcessor(
      ILogger<PublishQueueAckProcessor> logger)
   {
      _logger = logger;
   }
   
   public async Task Execute(
      ITcpServerConnection connection,
      [SocketPayload] QueuePublishConsumerAckMessage<JsonElement> message)
   {
      var queueServer = connection.CurrentServer.GetMetadata<CacheQueueServer>();
      using var scope = _logger.BeginScope(queueServer.NodeName);
      
      if (!queueServer.Options.IsClustered)
      {
         if (queueServer.QueueRegistry.GetLocalQueue(message.QueueName) is not { } queue)
         {
            return;
         }
         
         if (!queue.AckContainer.TrySetResult(message.AckRequestId, message))
         {
            QueuePublishAckMessage fallbackMessage = new()
            {
               AckRequestId = message.RequestId,
               IsPublished = true,
            };
            connection.Send(QueueEventNames.PublishAck, fallbackMessage);
         }
      }
   }
}