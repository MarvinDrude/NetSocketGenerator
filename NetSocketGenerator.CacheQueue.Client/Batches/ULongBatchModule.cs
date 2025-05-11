namespace NetSocketGenerator.CacheQueue.Client.Batches;

public sealed class ULongBatchModule(BatchBuilder builder)
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
      builder.AddCommand(new AddULongCommand()
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
         StoreTypeSet = StoreTypes.ULong
      });
      return builder;
   }
   
   public BatchBuilder Get(string keyName)
   {
      builder.AddCommand(new GetULongCommand()
      {
         KeyName = keyName,
         AwaitsAck = false
      });
      return builder;
   }
   
   public BatchBuilder Set(string keyName, ulong value)
   {
      builder.AddCommand(new SetULongCommand()
      {
         KeyName = keyName,
         Value = value,
         AwaitsAck = false
      });
      return builder;
   }
}