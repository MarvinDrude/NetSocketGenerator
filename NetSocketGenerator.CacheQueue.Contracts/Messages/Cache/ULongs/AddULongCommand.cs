namespace NetSocketGenerator.CacheQueue.Contracts.Messages.Cache.ULongs;

public sealed class AddULongCommand : BaseCommand
{
   public override string StoreType => StoreTypes.ULong;

   public required long Value { get; set; }
   
   public override string ToString()
   {
      return $"{base.ToString()}[ADD] = '{Value}'";
   }
}

public sealed class AddULongCommandAck : AckMessageBase
{
   public required ulong NewValue { get; set; }
}