namespace NetSocketGenerator.CacheQueue.Contracts.Messages.Cache.Longs;

public sealed class GetLongCommand : BaseCommand
{
   public override string StoreType => StoreTypes.Long;
   
   public override string ToString()
   {
      return $"{base.ToString()}[GET]";
   }
}

public sealed class GetLongCommandAck : AckMessageBase
{
   public long? Value { get; set; }
}