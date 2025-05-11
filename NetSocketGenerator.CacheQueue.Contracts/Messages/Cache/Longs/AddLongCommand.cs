namespace NetSocketGenerator.CacheQueue.Contracts.Messages.Cache.Longs;

public sealed class AddLongCommand : BaseCommand
{
   public override string StoreType => StoreTypes.Long;

   public required long Value { get; set; }
   
   public override string ToString()
   {
      return $"{base.ToString()}[ADD] = '{Value}'";
   }
}

public sealed class AddLongCommandAck : AckMessageBase
{
   public required long NewValue { get; set; }
}