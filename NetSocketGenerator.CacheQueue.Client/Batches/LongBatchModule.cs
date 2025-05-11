namespace NetSocketGenerator.CacheQueue.Client.Batches;

public class LongBatchModule(BatchBuilder builder)
{
   public BatchBuilder Increment(string keyName)
   {
      return Add(keyName, 1);
   }
   
   public BatchBuilder Decrement(string keyName)
   {
      return Add(keyName, -1);
   }
   
   public BatchBuilder Subtract(string keyName, long value)
   {
      return Add(keyName, -value);
   }
   
   public BatchBuilder Add(string keyName, long value)
   {
      builder.AddCommand(new AddLongCommand()
      {
         KeyName = keyName,
         Value = value,
         AwaitsAck = false,
      });
      return builder;
   }
   
   public BatchBuilder Delete(string keyName)
   {
      builder.AddCommand(new DeleteCommand()
      {
         KeyName = keyName,
         AwaitsAck = false,
         StoreTypeSet = StoreTypes.Long
      });
      return builder;
   }
   
   public BatchBuilder Get(string keyName)
   {
      builder.AddCommand(new GetLongCommand()
      {
         KeyName = keyName,
         AwaitsAck = false
      });
      return builder;
   }
   
   public BatchBuilder Set(string keyName, long value)
   {
      builder.AddCommand(new SetLongCommand()
      {
         KeyName = keyName,
         Value = value,
         AwaitsAck = false
      });
      return builder;
   }
}