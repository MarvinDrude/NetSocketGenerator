namespace NetSocketGenerator.CacheQueue.Bucketing.Stores;

public sealed class LongStore : IStore
{
   private readonly Dictionary<string, long?> _store = [];
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
         DeleteCommand deleteCommand => HandleDelete(deleteCommand, command),
         AddLongCommand addCommand => HandleAdd(addCommand, command),
         _ => false
      };
   }

   private bool HandleAdd(AddLongCommand source, BucketCommand bucketCommand)
   {
      if (!_store.TryGetValue(source.KeyName, out var value))
      {
         value = _store[source.KeyName] = 0;
      }

      value += source.Value;
      _store[source.KeyName] = value;
      
      _bucketExecutor.Server.AckContainer.TrySetResult<AckMessageBase>(bucketCommand.Id, new AddLongCommandAck()
      {
         AckRequestId = source.RequestId,
         NewValue = value ?? 0
      });
      
      return true;
   }
   
   private bool HandleGet(GetLongCommand source, BucketCommand bucketCommand)
   {
      var value = _store.GetValueOrDefault(source.KeyName);
      _bucketExecutor.Server.AckContainer.TrySetResult<AckMessageBase>(bucketCommand.Id, new GetLongCommandAck()
      {
         AckRequestId = source.RequestId,
         Value = value
      });
      
      return true;
   }
   
   private bool HandleSet(SetLongCommand source, BucketCommand bucketCommand)
   {
      _store[source.KeyName] = source.Value;
      _bucketExecutor.Server.AckContainer.TrySetResult<AckMessageBase>(bucketCommand.Id, new SetLongCommandAck()
      {
         AckRequestId = source.RequestId,
         Value = source.Value
      });
      
      return true;
   }
   
   private bool HandleDelete(DeleteCommand source, BucketCommand bucketCommand)
   {
      _bucketExecutor.Server.AckContainer.TrySetResult<AckMessageBase>(bucketCommand.Id, new DeleteCommandAck()
      {
         AckRequestId = source.RequestId,
         WasFound = _store.Remove(source.KeyName),
      });
      
      return true;
   }
}