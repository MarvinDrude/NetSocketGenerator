namespace NetSocketGenerator.Acknowledge;

public sealed class AckSource<T> : IAckSource
{
   public Task<T> Task => _tcs.Task;
   
   private readonly TaskCompletionSource<T> _tcs = new();

   public bool SetResult(T result)
   {
      return _tcs.TrySetResult(result);
   }

   public bool SetResult(object boxed)
   {
      return boxed is T result 
         && _tcs.TrySetResult(result);
   }
}