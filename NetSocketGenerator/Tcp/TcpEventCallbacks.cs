namespace NetSocketGenerator.Tcp;

/// <summary>
/// Encapsulates a set of callback functions to handle TCP events such as connection,
/// disconnection, and frame reception.
/// </summary>
/// <remarks>
/// This class provides a way to define asynchronous callback handlers for different events that occur
/// during the lifecycle of a TCP connection, including when a connection is established,
/// when it is disconnected, and when a frame is received.
/// </remarks>
public sealed class TcpEventCallbacks
{
   /// <summary>
   /// Gets or initializes the callback invoked when a TCP frame is received.
   /// </summary>
   /// <remarks>
   /// This property represents an asynchronous function that is triggered whenever a TCP frame is successfully received.
   /// The delegate accepts two parameters: the TCP connection that received the frame and the frame itself.
   /// </remarks>
   /// <value>
   /// A <see cref="Func{ITcpConnection, ITcpFrame, Task}"/> representing the callback function.
   /// If set to <c>null</c>, no action will be taken when a frame is received.
   /// </value>
   public Func<ITcpConnection, ITcpFrame, Task>? OnFrameReceived { get; init; }

   /// <summary>
   /// Gets or initializes the callback invoked when a TCP connection is established.
   /// </summary>
   /// <remarks>
   /// This property represents an asynchronous function that is triggered whenever a new TCP connection
   /// has been successfully established. The delegate accepts one parameter, which is the established TCP connection.
   /// </remarks>
   /// <value>
   /// A <see cref="Func{ITcpConnection, Task}"/> representing the callback function.
   /// If set to <c>null</c>, no action will be taken when a connection is established.
   /// </value>
   public Func<ITcpConnection, Task>? OnConnected { get; init; }

   /// <summary>
   /// Gets or initializes the callback invoked when a TCP connection is disconnected.
   /// </summary>
   /// <remarks>
   /// This property represents an asynchronous function that is triggered whenever a TCP connection is terminated.
   /// The delegate accepts a single parameter: the TCP connection that has been disconnected.
   /// </remarks>
   /// <value>
   /// A <see cref="Func{ITcpConnection, Task}"/> representing the callback function.
   /// If set to <c>null</c>, no action will be taken when a connection is disconnected.
   /// </value>
   public Func<ITcpConnection, Task>? OnDisconnected { get; init; }
}