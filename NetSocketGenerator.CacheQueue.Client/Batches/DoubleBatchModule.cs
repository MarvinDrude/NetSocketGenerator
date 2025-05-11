namespace NetSocketGenerator.CacheQueue.Client.Batches;

public sealed class DoubleBatchModule(BatchBuilder builder)
{
   public BatchBuilder Increment(string keyName)
   {
      return Add(keyName, 1);
   }
   
   public BatchBuilder Decrement(string keyName)
   {
      return Add(keyName, -1);
   }
   
   public BatchBuilder Subtract(string keyName, double value)
   {
      return Add(keyName, -value);
   }
   
   public BatchBuilder Add(string keyName, double value)
   {
      builder.AddCommand(new AddDoubleCommand()
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
         StoreTypeSet = StoreTypes.Double
      });
      return builder;
   }
   
   public BatchBuilder Get(string keyName)
   {
      builder.AddCommand(new GetDoubleCommand()
      {
         KeyName = keyName,
         AwaitsAck = false
      });
      return builder;
   }
   
   public BatchBuilder Set(string keyName, double value)
   {
      builder.AddCommand(new SetDoubleCommand()
      {
         KeyName = keyName,
         Value = value,
         AwaitsAck = false
      });
      return builder;
   }
}