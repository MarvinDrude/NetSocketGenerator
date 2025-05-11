namespace NetSocketGenerator.CacheQueue.Contracts.Messages.Cache.Doubles;

public sealed class AddDoubleCommand : BaseCommand
{
   public override string StoreType => StoreTypes.Double;

   public required double Value { get; set; }
   
   public override string ToString()
   {
      return $"{base.ToString()}[ADD] = '{Value}'";
   }
}

public sealed class AddDoubleCommandAck : AckMessageBase
{
   public required double NewValue { get; set; }
}