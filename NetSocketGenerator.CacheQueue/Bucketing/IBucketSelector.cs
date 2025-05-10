namespace NetSocketGenerator.CacheQueue.Bucketing;

public interface IBucketSelector : IDisposable
{
   public BucketExecutor ChooseBucketExecutor(string keyName);
   
   public void RemoveKeyExecutor(string keyName);
}