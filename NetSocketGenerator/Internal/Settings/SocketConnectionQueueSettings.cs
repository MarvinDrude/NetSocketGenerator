namespace NetSocketGenerator.Internal.Settings;

/// <summary>
/// Represents the configuration settings for a socket connection queue,
/// including memory management, scheduling policies, and pipe options for
/// handling data transmission.
/// </summary>
/// <remarks>
/// This class inherits from <see cref="ConnectionQueueSettings"/> and
/// introduces a required scheduler for managing connection task execution.
/// It is used internally to configure behavior specific to socket-based
/// connections, such as defining how data is received and transmitted via pipes.
/// </remarks>
internal sealed class SocketConnectionQueueSettings 
   : ConnectionQueueSettings
{
   /// <summary>
   /// Gets or initializes the <see cref="PipeScheduler"/> responsible for managing
   /// task execution in the socket connection queue.
   /// </summary>
   /// <remarks>
   /// The scheduler determines how connection-related tasks are scheduled
   /// and executed. It is an essential component in managing task parallelism
   /// and the effective utilization of system resources during socket communication.
   /// </remarks>
   public required PipeScheduler Scheduler { get; init; }
}