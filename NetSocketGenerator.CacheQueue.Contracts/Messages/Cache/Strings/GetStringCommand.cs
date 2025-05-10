namespace NetSocketGenerator.CacheQueue.Contracts.Messages.Cache.Strings;

public sealed class GetStringCommand : BaseCommand
{
   public override string StoreType => StoreTypes.String;
   
   public override string ToString()
   {
      return $"{base.ToString()}[GET]";
   }
}

public sealed class GetStringCommandAck : AckMessageBase
{
   public string? Value { get; set; }
}