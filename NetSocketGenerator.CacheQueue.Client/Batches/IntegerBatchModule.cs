namespace NetSocketGenerator.CacheQueue.Client.Batches;

public sealed class IntegerBatchModule(BatchBuilder builder)
{
   public BatchBuilder Increment(string keyName)
   {
      return Add(keyName, 1);
   }
   
   public BatchBuilder Decrement(string keyName)
   {
      return Add(keyName, -1);
   }
   
   public BatchBuilder Subtract(string keyName, int value)
   {
      return Add(keyName, -value);
   }
   
   public BatchBuilder Add(string keyName, int value)
   {
      builder.AddCommand(new AddIntegerCommand()
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
         StoreTypeSet = StoreTypes.Integer
      });
      return builder;
   }
   
   public BatchBuilder Get(string keyName)
   {
      builder.AddCommand(new GetIntegerCommand()
      {
         KeyName = keyName,
         AwaitsAck = false
      });
      return builder;
   }
   
   public BatchBuilder Set(string keyName, int value)
   {
      builder.AddCommand(new SetIntegerCommand()
      {
         KeyName = keyName,
         Value = value,
         AwaitsAck = false
      });
      return builder;
   }
}