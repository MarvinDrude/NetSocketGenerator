namespace NetSocketGenerator.CacheQueue.Contracts.Messages.Cache;

public sealed class DeleteCommand : BaseCommand
{
   public override string StoreType => StoreTypeSet;
   
   public required string StoreTypeSet { get; set; }
   
   public override string ToString()
   {
      return $"{base.ToString()}[DELETE]";
   }
}

public sealed class DeleteCommandAck : AckMessageBase
{
   public bool WasFound { get; set; }
}