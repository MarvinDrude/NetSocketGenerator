namespace NetSocketGenerator.CacheQueue.Client.Batches;

public sealed class StringBatchModule(BatchBuilder builder)
{
   public BatchBuilder Delete(string keyName)
   {
      builder.AddCommand(new DeleteCommand()
      {
         KeyName = keyName,
         AwaitsAck = false,
         StoreTypeSet = StoreTypes.String
      });
      return builder;
   }
   
   public BatchBuilder Get(string keyName)
   {
      builder.AddCommand(new GetStringCommand()
      {
         KeyName = keyName,
         AwaitsAck = false
      });
      return builder;
   }
   
   public BatchBuilder Set(string keyName, string value)
   {
      builder.AddCommand(new SetStringCommand()
      {
         KeyName = keyName,
         Value = value,
         AwaitsAck = false
      });
      return builder;
   }
}