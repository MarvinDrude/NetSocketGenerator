﻿namespace NetSocketGenerator.CacheQueue.Bucketing;

public sealed class RobinBucketSelector : IBucketSelector
{
   private readonly CacheQueueServerOptions _options;
   private readonly ConcurrentDictionary<string, BucketExecutor> _keyToBucketExecutor = [];
   
   private ulong _bucketIndex;
   private readonly ulong _bucketCount;
   private readonly BucketExecutor[] _bucketExecutors;
   
   public RobinBucketSelector(
      CacheQueueServer server,
      CacheQueueServerOptions options)
   {
      _options = options;

      _bucketExecutors = new BucketExecutor[options.BucketCount];
      _bucketCount = (ulong)options.BucketCount;
      
      for (var i = 0; i < options.BucketCount; i++)
      {
         _bucketExecutors[i] = new BucketExecutor(
            options.ServiceProvider.GetRequiredService<ILogger<BucketExecutor>>(),
            server, i);
      }
   }
   
   public BucketExecutor ChooseBucketExecutor(string keyName)
   {
      return _keyToBucketExecutor.GetOrAdd(keyName, _ =>
      {
         var bucketIndex = Interlocked.Increment(ref _bucketIndex) % _bucketCount;
         var bucketExecutor = _bucketExecutors[bucketIndex];
         
         return bucketExecutor;
      });
   }

   public void RemoveKeyExecutor(string keyName)
   {
      _keyToBucketExecutor.TryRemove(keyName, out _);
   }

   public void Dispose()
   {
      foreach (var bucketExecutor in _bucketExecutors)
      {
         bucketExecutor.Dispose();
      }
      _keyToBucketExecutor.Clear();
   }
}