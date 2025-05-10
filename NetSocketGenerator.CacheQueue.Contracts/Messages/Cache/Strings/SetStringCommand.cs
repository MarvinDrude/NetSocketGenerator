namespace NetSocketGenerator.CacheQueue.Contracts.Messages.Cache.Strings;

public sealed class SetStringCommand : BaseCommand
{
   public override string StoreType => StoreTypes.String;

   public required string Value { get; set; }
   
   public override string ToString()
   {
      return $"{base.ToString()}[SET] = '{Value}'";
   }
}

public sealed class SetStringCommandAck : AckMessageBase
{
   public string? Value { get; set; }
}