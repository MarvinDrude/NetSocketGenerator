namespace NetSocketGenerator.Internal.Options;

/// <summary>
/// Provides configuration options specific to socket-based connection management.
/// Extends <see cref="ConnectionOptions"/> to include socket-related behavior and resource allocation settings.
/// </summary>
/// <remarks>
/// This class is designed for configuring and managing the behavior of socket connections. It defines
/// settings such as memory pool allocation, pipe scheduling, and buffer constraints for read and write operations.
/// </remarks>
/// <seealso cref="ConnectionOptions"/>
public sealed class SocketConnectionOptions : ConnectionOptions
{
   /// <summary>
   /// Creates and configures connection queue settings tailored for socket-based connections.
   /// </summary>
   /// <returns>
   /// A configured instance of <see cref="ConnectionQueueSettings"/> containing memory pool,
   /// scheduler, and pipe options for managing buffer sizes and synchronization behavior.
   /// </returns>
   internal override ConnectionQueueSettings CreateQueueSettings() 
   {
      var memoryPool = new PinnedBlockMemoryPool();
      var scheduler = new IoQueue();

      var maxReadBufferSize = MaxReadBufferSize ?? 0;
      var maxWriteBufferSize = MaxWriteBufferSize ?? 0;

      return new SocketConnectionQueueSettings() 
      {
         MemoryPool = memoryPool,
         Scheduler = scheduler,
         
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