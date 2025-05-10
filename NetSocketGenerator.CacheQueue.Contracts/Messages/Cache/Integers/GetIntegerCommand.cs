namespace NetSocketGenerator.CacheQueue.Contracts.Messages.Cache.Integers;

public sealed class GetIntegerCommand : BaseCommand
{
   public override string StoreType => StoreTypes.Integer;
   
   public override string ToString()
   {
      return $"{base.ToString()}[GET]";
   }
}

public sealed class GetIntegerCommandAck : AckMessageBase
{
   public int? Value { get; set; }
}