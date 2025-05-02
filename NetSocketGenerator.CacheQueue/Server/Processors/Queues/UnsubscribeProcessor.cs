
namespace NetSocketGenerator.CacheQueue.Server.Processors.Queues;

[SocketProcessor(
   EventNamePattern = QueueEventNames.Unsubscribe,
   RegistrationGroups = ["Queue"],
   IncludeClient = false
)]
public sealed partial class UnsubscribeProcessor
{
   private readonly ILogger<UnsubscribeProcessor> _logger;
   
   public UnsubscribeProcessor(
      ILogger<UnsubscribeProcessor> logger)
   {
      _logger = logger;
   }
   
   public Task Execute(
      ITcpServerConnection connection,
      [SocketPayload] QueueUnsubscribeMessage message)
   {
      var queueServer = connection.CurrentServer.GetMetadata<CacheQueueServer>();
      using var scope = _logger.BeginScope(queueServer.NodeName);
      
      LogQueueUnsubscribe(message.QueueName);

      if (!queueServer.Options.IsClustered)
      {
         if (queueServer.QueueRegistry.GetLocalQueue(message.QueueName) 
             is not { } queue)
         {
            if (message.AwaitsAck)
            {
               connection.Send(QueueEventNames.Unsubscribe, 
                  message.CreateAckMessage(new QueueUnsubscribeAckMessage()
                  {
                     IsFound = false,
                     IsUnsubscribed = true
                  }));
            }
            return Task.CompletedTask;
         }
         
         var found = queue.RemoveLocalSubscription(connection.Id);
         
         if (message.AwaitsAck)
         {
            connection.Send(QueueEventNames.Unsubscribe, 
               message.CreateAckMessage(new QueueUnsubscribeAckMessage()
               {
                  IsFound = found,
                  IsUnsubscribed = true
               }));
         }
      }
      
      return Task.CompletedTask;
   }
}