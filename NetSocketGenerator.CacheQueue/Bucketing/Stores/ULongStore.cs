namespace NetSocketGenerator.CacheQueue.Bucketing.Stores;

public sealed class ULongStore : IStore
{
   private readonly Dictionary<string, ulong?> _store = [];
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
         DeleteCommand deleteCommand => HandleDelete(deleteCommand, command),
         AddULongCommand addCommand => HandleAdd(addCommand, command),
         _ => false
      };
   }
   
   private bool HandleAdd(AddULongCommand source, BucketCommand bucketCommand)
   {
      if (!_store.TryGetValue(source.KeyName, out var value))
      {
         value = _store[source.KeyName] = 0;
      }

      if (source.Value < 0)
      {
         value -= (ulong)(source.Value * -1);
      }
      else
      {
         value += (ulong)source.Value;
      }
      
      _store[source.KeyName] = value;
      
      _bucketExecutor.Server.AckContainer.TrySetResult<AckMessageBase>(source.RequestId, new AddULongCommandAck()
      {
         AckRequestId = source.RequestId,
         NewValue = value ?? 0
      });
      
      return true;
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
   
   private bool HandleDelete(DeleteCommand source, BucketCommand bucketCommand)
   {
      _bucketExecutor.Server.AckContainer.TrySetResult<AckMessageBase>(source.RequestId, new DeleteCommandAck()
      {
         AckRequestId = source.RequestId,
         WasFound = _store.Remove(source.KeyName),
      });
      
      return true;
   }
}