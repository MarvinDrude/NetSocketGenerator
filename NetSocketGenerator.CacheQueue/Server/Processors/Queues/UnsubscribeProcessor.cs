namespace NetSocketGenerator.CacheQueue.Server.Processors.Queues;

[SocketProcessor(
   EventNamePattern = QueueEventNames.Unsubscribe,
   RegistrationGroups = ["Queue"],
   IncludeClient = false
)]
public sealed partial class UnsubscribeProcessor
{
   public Task Execute(
      ITcpServerConnection connection,
      [SocketPayload] QueueUnsubscribeMessage message)
   {
      var queueServer = connection.CurrentServer.GetMetadata<CacheQueueServer>();

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
         
         queue.AddLocalSubscription(connection.Id);
         
         if (message.AwaitsAck)
         {
            connection.Send(QueueEventNames.Unsubscribe, 
               message.CreateAckMessage(new QueueUnsubscribeAckMessage()
               {
                  IsFound = true,
                  IsUnsubscribed = true
               }));
         }
      }
      
      return Task.CompletedTask;
   }
}