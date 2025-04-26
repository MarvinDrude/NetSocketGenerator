namespace NetSocketGenerator.Internal.Settings;

/// <summary>
/// Represents the settings used to configure the connection queue.
/// </summary>
/// <remarks>
/// This class provides initialization for pipeline options related to
/// receiving and sending data, as well as a memory pool for efficient
/// memory management. It is utilized internally to define queue-specific
/// behaviors and resources required for managing connections.
/// </remarks>
internal class ConnectionQueueSettings : IDisposable
{
   /// <summary>
   /// Gets or sets the <see cref="PipeOptions"/> for configuring the receiving pipeline.
   /// </summary>
   /// <remarks>
   /// <para>Defines the options utilized for managing data reception in the pipeline. This includes
   /// configuration for buffer size, scheduling, and synchronization context to optimize performance
   /// in a high-throughput environment.</para>
   /// <para>The <see cref="ReceiveOptions"/> property is typically initialized with specific values
   /// that dictate how data is read and processed from underlying streams or sockets. Internally, it
   /// supports efficient queue and memory management tailored for connection handling.</para>
   /// </remarks>
   public required PipeOptions ReceiveOptions { get; init; }

   /// <summary>
   /// Gets or sets the <see cref="PipeOptions"/> for configuring the sending pipeline.
   /// </summary>
   /// <remarks>
   /// Defines the options utilized for managing data transmission in the pipeline. This includes
   /// configuration for buffer size, scheduling, and synchronization context to ensure optimal
   /// performance in high-throughput scenarios.
   /// The <see cref="SendOptions"/> property is typically used to initialize specific values
   /// related to how data is written and transmitted via underlying streams or sockets. Internally,
   /// it supports efficient data flow and memory management tailored for outbound communication.
   /// </remarks>
   public required PipeOptions SendOptions { get; init; }

   /// <summary>
   /// Gets or sets the <see cref="MemoryPool{T}"/> used for efficient memory allocation
   /// and management during data processing operations.
   /// </summary>
   /// <remarks>
   /// The <see cref="MemoryPool"/> property is used to allocate memory buffers for
   /// pipelines and other connection-related operations. This property provides an
   /// abstraction for pooling memory, which helps optimize performance, reduce garbage
   /// collection overhead, and minimize memory allocation costs.
   /// It is particularly useful in scenarios where high-throughput data processing
   /// is required, such as network connections, where efficient memory usage
   /// directly impacts overall system performance. The allocated memory pool allows
   /// reuse of buffers, avoiding unnecessary allocations to enhance efficiency.
   /// </remarks>
   public required MemoryPool<byte> MemoryPool { get; init; }

   /// <summary>
   /// Indicates whether the current instance has been disposed.
   /// </summary>
   /// <remarks>
   /// This field is used internally to track the disposal state of the object and prevent
   /// multiple calls to the disposal logic. When set to <c>true</c>, it ensures that
   /// resources such as memory pools are released properly and no further operations
   /// related to the instance are performed.
   /// </remarks>
   private bool _disposed = false;

   /// <summary>
   /// Releases all resources used by the object.
   /// </summary>
   /// <remarks>
   /// This method is used to free resources such as memory pools and other
   /// disposable resources held by the object. Failure to call this method may result
   /// in resource leaks. After disposing, the object should not be used.
   /// </remarks>
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