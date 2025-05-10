namespace NetSocketGenerator.CacheQueue.Bucketing.Stores;

public sealed class ULongStore : IStore
{
   private readonly Dictionary<string, ulong> _store = [];
   private readonly BucketExecutor _bucketExecutor;
   
   public ULongStore(BucketExecutor bucketExecutor)
   {
      _bucketExecutor = bucketExecutor;
   }
   
   public bool Handle(BucketCommand command)
   {
      return command.SourceCommand switch
      {
         GetULongCommand getCommand => HandleGet(getCommand, command),
         SetULongCommand setCommand => HandleSet(setCommand, command),
         _ => false
      };
   }

   private bool HandleGet(GetULongCommand source, BucketCommand bucketCommand)
   {
      var value = _store.GetValueOrDefault(source.KeyName);
      _bucketExecutor.Server.AckContainer.TrySetResult<AckMessageBase>(source.RequestId, new GetULongCommandAck()
      {
         AckRequestId = source.RequestId,
         Value = value
      });
      
      return true;
   }
   
   private bool HandleSet(SetULongCommand source, BucketCommand bucketCommand)
   {
      _store[source.KeyName] = source.Value;
      _bucketExecutor.Server.AckContainer.TrySetResult<AckMessageBase>(source.RequestId, new SetULongCommandAck()
      {
         AckRequestId = source.RequestId,
         Value = source.Value
      });
      
      return true;
   }
}