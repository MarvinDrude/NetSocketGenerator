namespace NetSocketGenerator.CacheQueue.Contracts.Messages.Cache.Doubles;

public sealed class SetDoubleCommand : BaseCommand
{
   public override string StoreType => StoreTypes.Double;

   public required double Value { get; set; }
   
   public override string ToString()
   {
      return $"{base.ToString()}[SET] = '{Value}'";
   }
}

public sealed class SetDoubleCommandAck : AckMessageBase
{
   public double? Value { get; set; }
}