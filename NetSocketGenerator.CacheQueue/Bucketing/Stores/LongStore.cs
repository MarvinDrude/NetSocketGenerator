namespace NetSocketGenerator.CacheQueue.Bucketing.Stores;

public sealed class LongStore : IStore
{
   private readonly Dictionary<string, long> _store = [];
   private readonly BucketExecutor _bucketExecutor;
   
   public LongStore(BucketExecutor bucketExecutor)
   {
      _bucketExecutor = bucketExecutor;
   }
   
   public bool Handle(BucketCommand command)
   {
      return command.SourceCommand switch
      {
         GetLongCommand getCommand => HandleGet(getCommand, command),
         SetLongCommand setCommand => HandleSet(setCommand, command),
         _ => false
      };
   }

   private bool HandleGet(GetLongCommand source, BucketCommand bucketCommand)
   {
      var value = _store.GetValueOrDefault(source.KeyName);
      _bucketExecutor.Server.AckContainer.TrySetResult<AckMessageBase>(source.RequestId, new GetLongCommandAck()
      {
         AckRequestId = source.RequestId,
         Value = value
      });
      
      return true;
   }
   
   private bool HandleSet(SetLongCommand source, BucketCommand bucketCommand)
   {
      _store[source.KeyName] = source.Value;
      _bucketExecutor.Server.AckContainer.TrySetResult<AckMessageBase>(source.RequestId, new SetLongCommandAck()
      {
         AckRequestId = source.RequestId,
         Value = source.Value
      });
      
      return true;
   }
}