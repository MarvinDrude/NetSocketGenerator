namespace NetSocketGenerator.CacheQueue.Contracts.Messages.Cache.ULongs;

public sealed class GetULongCommand : BaseCommand
{
   public override string StoreType => StoreTypes.ULong;
   
   public override string ToString()
   {
      return $"{base.ToString()}[GET]";
   }
}

public sealed class GetULongCommandAck : AckMessageBase
{
   public ulong? Value { get; set; }
}