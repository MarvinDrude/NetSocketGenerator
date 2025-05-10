namespace NetSocketGenerator.CacheQueue.Contracts.Messages.Cache.Longs;

public class SetLongCommand : BaseCommand
{
   public override string StoreType => StoreTypes.Long;

   public required long Value { get; set; }
   
   public override string ToString()
   {
      return $"{base.ToString()}[SET] = '{Value}'";
   }
}

public sealed class SetLongCommandAck : AckMessageBase
{
   public long? Value { get; set; }
}