namespace NetSocketGenerator.Internal.Options;

/// <summary>
/// Represents configuration options for managing stream-based connections.
/// This class extends <see cref="ConnectionOptions"/> to specifically provide behavior
/// and management tailored for stream-oriented communication scenarios.
/// </summary>
public class StreamConnectionOptions: ConnectionOptions 
{
   /// <summary>
   /// Creates and returns an instance of <see cref="ConnectionQueueSettings"/> configured with the appropriate settings
   /// for stream-based connections, including memory pool allocation, receive and send pipe options.
   /// </summary>
   /// <returns>
   /// A <see cref="ConnectionQueueSettings"/> object configured with stream connection queue settings.
   /// </returns>
   internal override ConnectionQueueSettings CreateQueueSettings() 
   {
      var memoryPool = new PinnedBlockMemoryPool();

      var maxReadBufferSize = MaxReadBufferSize ?? 0;
      var maxWriteBufferSize = MaxWriteBufferSize ?? 0;

      return new StreamConnectionQueueSettings() 
      {
         MemoryPool = memoryPool,
         
         ReceiveOptions = new PipeOptions(
            memoryPool, PipeScheduler.ThreadPool, PipeScheduler.ThreadPool,
            maxReadBufferSize, maxReadBufferSize / 2,
            useSynchronizationContext: false),
         SendOptions = new PipeOptions(
            memoryPool, PipeScheduler.ThreadPool, PipeScheduler.ThreadPool,
            maxWriteBufferSize, maxWriteBufferSize / 2,
            useSynchronizationContext: false),
      };
   }
}