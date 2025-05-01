namespace NetSocketGenerator.CacheQueue.Contracts.Messages;

public class MessageBase
{
   public Guid RequestId { get; set; } = Guid.CreateVersion7();
   
   public TimeSpan AckTimeout { get; set; } = TimeSpan.FromSeconds(10);
   
   public bool AwaitsAck { get; set; }

   public T CreateAckMessage<T>(T ackMessage) 
      where T : AckMessageBase
   {
      ackMessage.AckRequestId = RequestId;
      return ackMessage;
   }
}