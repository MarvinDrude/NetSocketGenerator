namespace NetSocketGenerator.CacheQueue.Contracts.Messages.Cache.ULongs;

public sealed class SetULongCommand : BaseCommand
{
   public override string StoreType => StoreTypes.ULong;

   public required ulong Value { get; set; }
   
   public override string ToString()
   {
      return $"{base.ToString()}[SET] = '{Value}'";
   }
}

public sealed class SetULongCommandAck : AckMessageBase
{
   public ulong? Value { get; set; }
}