namespace NetSocketGenerator.CacheQueue.Contracts.Messages.Cache.Doubles;

public sealed class GetDoubleCommand : BaseCommand
{
   public override string StoreType => StoreTypes.Double;

   public override string ToString()
   {
      return $"{base.ToString()}[GET]";
   }
}

public sealed class GetDoubleCommandAck : AckMessageBase
{
   public double? Value { get; set; }
}