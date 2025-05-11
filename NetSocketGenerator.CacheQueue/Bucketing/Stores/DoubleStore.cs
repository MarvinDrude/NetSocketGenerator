namespace NetSocketGenerator.CacheQueue.Bucketing.Stores;

public sealed class DoubleStore : IStore
{
   private readonly Dictionary<string, double?> _store = [];
   private readonly BucketExecutor _bucketExecutor;
   
   public DoubleStore(BucketExecutor bucketExecutor)
   {
      _bucketExecutor = bucketExecutor;
   }
   
   public bool Handle(BucketCommand command)
   {
      return command.SourceCommand switch
      {
         GetDoubleCommand getCommand => HandleGet(getCommand, command),
         SetDoubleCommand setCommand => HandleSet(setCommand, command),
         DeleteCommand deleteCommand => HandleDelete(deleteCommand, command),
         AddDoubleCommand addCommand => HandleAdd(addCommand, command),
         _ => false
      };
   }

   private bool HandleAdd(AddDoubleCommand source, BucketCommand bucketCommand)
   {
      if (!_store.TryGetValue(source.KeyName, out var value))
      {
         value = _store[source.KeyName] = 0d;
      }

      value += source.Value;
      _store[source.KeyName] = value;
      
      _bucketExecutor.Server.AckContainer.TrySetResult<AckMessageBase>(bucketCommand.Id, new AddDoubleCommandAck()
      {
         AckRequestId = source.RequestId,
         NewValue = value ?? 0d
      });
      
      return true;
   }
   
   private bool HandleGet(GetDoubleCommand source, BucketCommand bucketCommand)
   {
      var value = _store.GetValueOrDefault(source.KeyName);
      _bucketExecutor.Server.AckContainer.TrySetResult<AckMessageBase>(bucketCommand.Id, new GetDoubleCommandAck()
      {
         AckRequestId = source.RequestId,
         Value = value
      });
      
      return true;
   }
   
   private bool HandleSet(SetDoubleCommand source, BucketCommand bucketCommand)
   {
      _store[source.KeyName] = source.Value;
      _bucketExecutor.Server.AckContainer.TrySetResult<AckMessageBase>(bucketCommand.Id, new SetDoubleCommandAck()
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