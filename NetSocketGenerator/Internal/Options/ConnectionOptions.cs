namespace NetSocketGenerator.Internal.Options;

/// <summary>
/// Represents the base set of configurable options for connection management in networking operations.
/// This class is designed to be extended to define specific connection behavior and constraints.
/// </summary>
public abstract class ConnectionOptions
{
   /// <summary>
   /// Gets or sets the number of IO queues to be used for connection processing.
   /// This property determines the level of parallelism for IO operations, and its default value
   /// is the lesser of the number of available processor cores or 24.
   /// </summary>
   public int IoQueueCount { get; set; } = Math.Min(Environment.ProcessorCount, 24);

   /// <summary>
   /// Gets or sets the maximum allowable size, in bytes, of the read buffer used
   /// for handling incoming data in networking operations. This property controls
   /// the amount of memory allocated for buffering incoming data before being processed.
   /// The default value is 1 MB. A null value indicates no limit is applied.
   /// </summary>
   public long? MaxReadBufferSize { get; set; } = 1024 * 1024;

   /// <summary>
   /// Gets or sets the maximum allowable size, in bytes, of the write buffer for a connection.
   /// This property is used to control the memory allocated for outgoing data and assists in managing backpressure.
   /// The default value is 64 * 1024 bytes (64 KB).
   /// </summary>
   public long? MaxWriteBufferSize { get; set; } = 64 * 1024;

   /// <summary>
   /// Creates and returns a new instance of <see cref="ConnectionQueueSettings"/>
   /// configured with properties defined by the subclass implementation.
   /// </summary>
   /// <returns>
   /// A new instance of <see cref="ConnectionQueueSettings"/> containing
   /// the configurations for connection queue settings such as buffer sizes,
   /// memory pool, and scheduler properties.
   /// </returns>
   internal abstract ConnectionQueueSettings CreateQueueSettings(); 
}