namespace NetSocketGenerator.CacheQueue.Contracts.Messages.Cache;

public sealed class BatchCommand : BaseCommand
{
   public required List<BaseCommand> Commands { get; set; }
   
   public override string ToString()
   {
      return $"{base.ToString()}[BATCH]";
   }
}

public sealed class BatchCommandAck : AckMessageBase
{
   public required List<AckMessageBase?> Acks { get; set; }
}