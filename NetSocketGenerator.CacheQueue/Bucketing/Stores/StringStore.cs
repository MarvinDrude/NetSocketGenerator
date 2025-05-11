
namespace NetSocketGenerator.CacheQueue.Bucketing.Stores;

public sealed class StringStore : IStore
{
   private readonly Dictionary<string, string> _store = [];
   private readonly BucketExecutor _bucketExecutor;
   
   public StringStore(BucketExecutor bucketExecutor)
   {
      _bucketExecutor = bucketExecutor;
   }
   
   public bool Handle(BucketCommand command)
   {
      return command.SourceCommand switch
      {
         GetStringCommand getCommand => HandleGet(getCommand, command),
         SetStringCommand setCommand => HandleSet(setCommand, command),
         DeleteCommand deleteCommand => HandleDelete(deleteCommand, command),
         _ => false
      };
   }

   private bool HandleGet(GetStringCommand source, BucketCommand bucketCommand)
   {
      var value = _store.GetValueOrDefault(source.KeyName);
      _bucketExecutor.Server.AckContainer.TrySetResult<AckMessageBase>(bucketCommand.Id, new GetStringCommandAck()
      {
         AckRequestId = source.RequestId,
         Value = value
      });
      
      return true;
   }
   
   private bool HandleSet(SetStringCommand source, BucketCommand bucketCommand)
   {
      _store[source.KeyName] = source.Value;
      _bucketExecutor.Server.AckContainer.TrySetResult<AckMessageBase>(bucketCommand.Id, new SetStringCommandAck()
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