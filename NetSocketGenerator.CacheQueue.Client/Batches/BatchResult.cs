namespace NetSocketGenerator.CacheQueue.Client.Batches;

public sealed class BatchResult(BatchCommandAck? ack)
{
   public bool HasAck => ack is not null;
   
   public T? GetAck<T>(int index) 
      where T : AckMessageBase
   {
      if (ack is null)
      {
         return null;
      }

      if (index < 0 || index >= ack.Acks.Count)
      {
         return null;
      }

      if (ack.Acks[index] is not T ackMessage)
      {
         return null;
      }
      
      return ackMessage;
   }
}