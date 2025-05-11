namespace NetSocketGenerator.CacheQueue.Contracts.Messages.Cache.Integers;

public sealed class AddIntegerCommand : BaseCommand
{
   public override string StoreType => StoreTypes.Integer;

   public required int Value { get; set; }
   
   public override string ToString()
   {
      return $"{base.ToString()}[ADD] = '{Value}'";
   }
}

public sealed class AddIntegerCommandAck : AckMessageBase
{
   public required int NewValue { get; set; }
}