
namespace NetSocketGenerator.CacheQueue.Bucketing;

public sealed class BucketExecutor : IDisposable
{
   private readonly Channel<BucketCommand> _commandChannel = Channel.CreateUnbounded<BucketCommand>();
   private readonly StringStore _stringStore = new();

   public 
   
   private void Handle(BucketCommand command)
   {
      _ = command.SourceCommand.StoreType switch
      {
         StoreTypes.String => _stringStore.Handle(command),
         _ => false
      };
   }
}