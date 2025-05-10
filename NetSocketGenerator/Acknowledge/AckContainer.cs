namespace NetSocketGenerator.Acknowledge;

public sealed class AckContainer : IDisposable
{
   public int PendingCount => _pendingAcks.Count;
   
   private readonly ConcurrentDictionary<Guid, IAckSource> _pendingAcks = [];

   public async Task<T?> Enqueue<T>(Guid id, TimeSpan timeout)
   {
      var ackSource = new AckSource<T>();
      _pendingAcks[id] = ackSource;

      try
      {
         var completedTask = await Task.WhenAny(
            ackSource.Task,
            Task.Delay(timeout));

         if (completedTask == ackSource.Task)
         {
            return await ackSource.Task;
         }
      }
      finally
      {
         _pendingAcks.TryRemove(id, out _);
      }
      
      return default;
   }

   public bool TrySetResult<T>(Guid id, T result)
   {
      if (_pendingAcks.TryRemove(id, out var ackSource)
          && ackSource is AckSource<T> source)
      {
         return source.SetResult(result);
      }

      return false;
   }

   public bool TrySetResult(Guid id, object boxed)
   {
      return _pendingAcks.TryRemove(id, out var ackSource) 
         && ackSource.SetResult(boxed);
   }
   
   public void Dispose()
   {
      _pendingAcks.Clear();
   }
}