namespace NetSocketGenerator.CacheQueue.Contracts.Messages;

public class MessageBase
{
   public Guid RequestId { get; init; } = Guid.CreateVersion7();

   public bool AwaitsAck { get; set; }

   public T CreateAckMessage<T>(T ackMessage) 
      where T : AckMessageBase
   {
      ackMessage.AckRequestId = RequestId;
      return ackMessage;
   }
}