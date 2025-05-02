
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
   
   public Task Execute(
      ITcpServerConnection connection,
      [SocketPayload] QueueSubscribeMessage message)
   {
      var queueServer = connection.CurrentServer.GetMetadata<CacheQueueServer>();

      if (!queueServer.Options.IsClustered)
      {
         if (queueServer.QueueRegistry.GetLocalQueue(message.QueueName) 
             is not { } queue)
         {
            if (message.AwaitsAck)
            {
               connection.Send(QueueEventNames.Subscribe, 
                  message.CreateAckMessage(new QueueSubscribeAckMessage()
                  {
                     IsFound = false,
                     IsSubscribed = false
                  }));
            }
            return Task.CompletedTask;
         }
         
         queue.AddLocalSubscription(connection.Id);
         
         if (message.AwaitsAck)
         {
            connection.Send(QueueEventNames.Subscribe, 
               message.CreateAckMessage(new QueueSubscribeAckMessage()
               {
                  IsFound = true,
                  IsSubscribed = true
               }));
         }
      }
      
      return Task.CompletedTask;
   }
}