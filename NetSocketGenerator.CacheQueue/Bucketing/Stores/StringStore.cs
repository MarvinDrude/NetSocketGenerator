
namespace NetSocketGenerator.CacheQueue.Bucketing.Stores;

public sealed class StringStore : IStore
{
   private readonly Dictionary<string, string> _store = [];
   
   public bool Handle(BucketCommand command)
   {
      return command.SourceCommand switch
      {
         GetStringCommand getCommand => HandleGet(getCommand, command),
         SetStringCommand setCommand => HandleSet(setCommand, command),
         _ => false
      };
   }

   private bool HandleGet(GetStringCommand source, BucketCommand bucketCommand)
   {
      if (bucketCommand.AckSource is not AckSource<GetStringCommandAck> ackSource)
      {
         return false;
      }

      var value = _store.GetValueOrDefault(source.KeyName);
      ackSource.SetResult(new GetStringCommandAck()
      {
         AckRequestId = source.RequestId,
         Value = value
      });
      
      return true;
   }
   
   private bool HandleSet(SetStringCommand source, BucketCommand bucketCommand)
   {
      if (bucketCommand.AckSource is not AckSource<SetStringCommandAck> ackSource)
      {
         return false;
      }

      _store[source.KeyName] = source.Value;
      ackSource.SetResult(new SetStringCommandAck()
      {
         AckRequestId = source.RequestId,
         Value = source.Value
      });
      
      return true;
   }
}