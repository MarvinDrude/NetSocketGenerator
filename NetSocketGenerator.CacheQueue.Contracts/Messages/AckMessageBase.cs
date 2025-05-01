namespace NetSocketGenerator.CacheQueue.Contracts.Messages;

public class AckMessageBase
{
   public Guid RequestId { get; set; } = Guid.CreateVersion7();
   
   public Guid AckRequestId { get; set; }
}