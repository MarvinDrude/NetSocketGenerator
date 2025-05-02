using System.Text.Json;

namespace NetSocketGenerator.CacheQueue.Client.Contexts;

public sealed class QueuePublishContext<T>
{
   public required CacheQueueClient Client { get; set; }
   
   public required T Message { get; set; }
   
   public required string QueueName { get; set; }
   
   public required QueuePublishMessage<JsonElement> RawMessage { get; set; }

   public Task<bool> Respond<TResponse>(TResponse response)
   {
      var message = new QueuePublishConsumerAckMessage<TResponse>()
      {
         AwaitsAck = true,
         Contents = response,
         AckRequestId = RawMessage.RequestId,
      };
      
      return Respond(message);
   }

   internal async Task<bool> Respond<TResponse>(QueuePublishConsumerAckMessage<TResponse> message)
   {
      var task = Client.AckContainer.Enqueue<QueuePublishAckMessage>(message.RequestId, Client.Options.ServerAckTimeout);
      
      message.AwaitsAck = true;
      Client.Tcp.Send(QueueEventNames.PublishAck, message);

      var result = await task;
      
      return result is { IsPublished: true };
   }
   
   public void RespondNoAck<TResponse>(TResponse response)
   {
      var message = new QueuePublishConsumerAckMessage<TResponse>()
      {
         AwaitsAck = false,
         Contents = response,
         AckRequestId = RawMessage.RequestId,
      };
      RespondNoAck(message);
   }

   internal void RespondNoAck<TResponse>(QueuePublishConsumerAckMessage<TResponse> message)
   {
      message.AwaitsAck = false;
      Client.Tcp.Send(QueueEventNames.PublishAck, message);
   }
}