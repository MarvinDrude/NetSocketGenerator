namespace NetSocketGenerator.CacheQueue.Contracts.Messages;

public class MessageBase
{
   public Guid RequestId { get; set; } = Guid.CreateVersion7();
   
   public bool AwaitsAck { get; set; }
   
   public T CreateAckMessage<T>(T ackMessage) 
      where T : AckMessageBase, new()
   {
      ackMessage.AckRequestId = RequestId;
      return ackMessage;
   }
}