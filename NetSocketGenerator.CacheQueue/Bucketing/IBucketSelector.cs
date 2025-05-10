namespace NetSocketGenerator.CacheQueue.Bucketing;

public interface IBucketSelector
{
   public BucketExecutor ChooseBucketExecutor(string keyName);
   
   public void RemoveKeyExecutor(string keyName);
}