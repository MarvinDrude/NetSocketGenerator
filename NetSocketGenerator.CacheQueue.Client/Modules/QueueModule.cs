namespace NetSocketGenerator.CacheQueue.Client.Modules;

public sealed class QueueModule(CacheQueueClient client)
{
   public Task<bool> Unsubscribe(string queueName)
   {
      var message = new QueueUnsubscribeMessage()
      {
         QueueName = queueName,
      };
      return Unsubscribe(message);
   }

   public async Task<bool> Unsubscribe(QueueUnsubscribeMessage message)
   {
      message.AwaitsAck = true;
      
      var task = client.AckContainer
         .Enqueue<QueueUnsubscribeAckMessage>(message.RequestId, client.Options.ServerAckTimeout);
      
      client.Tcp.Send(QueueEventNames.Unsubscribe, message);
      var result = await task;
      
      return result is { IsUnsubscribed: true };
   }
   
   public void UnsubscribeNoAck(string queueName)
   {
      var message = new QueueUnsubscribeMessage()
      {
         QueueName = queueName,
      };
      UnsubscribeNoAck(message);
   }

   public void UnsubscribeNoAck(QueueUnsubscribeMessage message)
   {
      message.AwaitsAck = false;
      client.Tcp.Send(QueueEventNames.Unsubscribe, message);
   }
   
   public Task<bool> Subscribe(string queueName)
   {
      var message = new QueueSubscribeMessage()
      {
         QueueName = queueName,
      };
      return Subscribe(message);
   }

   public async Task<bool> Subscribe(QueueSubscribeMessage message)
   {
      message.AwaitsAck = true;
      
      var task = client.AckContainer
         .Enqueue<QueueSubscribeAckMessage>(message.RequestId, client.Options.ServerAckTimeout);
      
      client.Tcp.Send(QueueEventNames.Subscribe, message);
      var result = await task;
      
      return result is { IsSubscribed: true };
   }
   
   public void SubscribeNoAck(string queueName)
   {
      var message = new QueueSubscribeMessage()
      {
         QueueName = queueName,
      };
      SubscribeNoAck(message);
   }

   public void SubscribeNoAck(QueueSubscribeMessage message)
   {
      message.AwaitsAck = false;
      client.Tcp.Send(QueueEventNames.Subscribe, message);
   }
   
   public Task<bool> Delete(string queueName)
   {
      var message = new QueueDeleteMessage()
      {
         QueueName = queueName,
      };
      return Delete(message);
   }

   public async Task<bool> Delete(QueueDeleteMessage message)
   {
      message.AwaitsAck = true;
      
      var task = client.AckContainer
         .Enqueue<QueueDeleteAckMessage>(message.RequestId, client.Options.ServerAckTimeout);
      
      client.Tcp.Send(QueueEventNames.Delete, message);
      var result = await task;
      
      return result is { IsDeleted: true };
   }
   
   public void DeleteNoAck(string queueName)
   {
      var message = new QueueDeleteMessage()
      {
         QueueName = queueName,
      };
      DeleteNoAck(message);
   }

   public void DeleteNoAck(QueueDeleteMessage message)
   {
      message.AwaitsAck = false;
      client.Tcp.Send(QueueEventNames.Delete, message);
   }
   
   public Task<bool> Create(string queueName)
   {
      var message = new QueueCreateMessage()
      {
         QueueName = queueName,
         SubscribeImmediately = false
      };
      
      return Create(message);
   }
   
   public async Task<bool> Create(QueueCreateMessage message)
   {
      message.AwaitsAck = true;
      
      var task = client.AckContainer
         .Enqueue<QueueCreateAckMessage>(message.RequestId, client.Options.ServerAckTimeout);
      
      client.Tcp.Send(QueueEventNames.Create, message);
      var result = await task;

      return result is not null;
   }

   public void CreateNoAck(string queueName)
   {
      var message = new QueueCreateMessage()
      {
         QueueName = queueName,
         SubscribeImmediately = false
      };
      CreateNoAck(message);
   }
   
   public void CreateNoAck(QueueCreateMessage message)
   {
      message.AwaitsAck = false;
      client.Tcp.Send(QueueEventNames.Create, message);
   }
}