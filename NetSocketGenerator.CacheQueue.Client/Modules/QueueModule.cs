using System.Text.Json;

namespace NetSocketGenerator.CacheQueue.Client.Modules;

public sealed class QueueModule(CacheQueueClient client)
{
   internal ConcurrentDictionary<string, IDelegateContainer> Handlers { get; } = [];
   
   public void AddHandler<T>(string queueName, QueuePublishDelegate<T> handler)
   {
      if (!Handlers.TryGetValue(queueName, out var container))
      {
         container = Handlers[queueName] = new DelegateContainer<T>();
      }

      if (container is not DelegateContainer<T> containerTyped)
      {
         return;
      }

      containerTyped.Add(handler);
   }
   
   public Task<TOutput?> PublishAndReceive<TInput, TOutput>(string queueName, TInput contents)
   {
      var message = new QueuePublishMessage<TInput>()
      {
         ConsumerAck = true,
         Contents = contents,
         ConsumerAckTimeout = client.Options.QueueConsumerAckTimeout,
         QueueName = queueName
      };
      return PublishAndReceive<TInput, TOutput>(message);
   }

   public async Task<TOutput?> PublishAndReceive<TInput, TOutput>(QueuePublishMessage<TInput> message)
   {
      message.AwaitsAck = false;
      message.ConsumerAck = true;
      
      var task = client.AckContainer
         .Enqueue<QueuePublishConsumerAckMessage<JsonElement>>(message.RequestId, client.Options.ServerAckTimeout);
      
      client.Tcp.Send(QueueEventNames.Publish, message);
      var result = await task;

      return result is null 
         ? default 
         : result.Contents.Deserialize<TOutput>();
   }
   
   public Task<bool> Publish<T>(string queueName, T contents)
   {
      var message = new QueuePublishMessage<T>()
      {
         QueueName = queueName,
         Contents = contents,
         ConsumerAckTimeout = client.Options.QueueConsumerAckTimeout,
         ConsumerAck = false,
      };
      return Publish(message);
   }

   public async Task<bool> Publish<T>(QueuePublishMessage<T> message)
   {
      message.AwaitsAck = true;
      message.ConsumerAck = false;
      
      var task = client.AckContainer
         .Enqueue<QueuePublishAckMessage>(message.RequestId, client.Options.ServerAckTimeout);
      
      client.Tcp.Send(QueueEventNames.Publish, message);
      var result = await task;
      
      return result is { IsPublished: true };
   }
   
   public void PublishNoAck<T>(string queueName, T contents)
   {
      var message = new QueuePublishMessage<T>()
      {
         QueueName = queueName,
         ConsumerAckTimeout = client.Options.QueueConsumerAckTimeout,
         ConsumerAck = false,
         Contents = contents
      };
      PublishNoAck(message);
   }

   public void PublishNoAck<T>(QueuePublishMessage<T> message)
   {
      message.AwaitsAck = false;
      message.ConsumerAck = false;
      client.Tcp.Send(QueueEventNames.Publish, message);
   }
   
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