namespace NetSocketGenerator.Internal.Settings;

internal class ConnectionQueueSettings : IDisposable
{
   public required PipeOptions ReceiveOptions { get; init; }
   
   public required PipeOptions SendOptions { get; init; }
   
   public required MemoryPool<byte> MemoryPool { get; init; }

   private bool _disposed = false;

   public virtual void Dispose()
   {
      if (_disposed)
      {
         return;
      }
      
      _disposed = true;
      MemoryPool?.Dispose();
   }
}