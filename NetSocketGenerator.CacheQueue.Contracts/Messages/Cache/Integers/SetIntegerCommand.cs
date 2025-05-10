namespace NetSocketGenerator.CacheQueue.Contracts.Messages.Cache.Integers;

public sealed class SetIntegerCommand : BaseCommand
{
   public override string StoreType => StoreTypes.Integer;

   public required int Value { get; set; }
   
   public override string ToString()
   {
      return $"{base.ToString()}[SET] = '{Value}'";
   }
}

public sealed class SetIntegerCommandAck : AckMessageBase
{
   public int? Value { get; set; }
}