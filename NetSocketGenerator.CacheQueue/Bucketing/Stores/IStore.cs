namespace NetSocketGenerator.CacheQueue.Bucketing.Stores;

public interface IStore
{
   public bool Handle(BucketCommand command);
}