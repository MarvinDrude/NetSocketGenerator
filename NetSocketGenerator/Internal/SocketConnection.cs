namespace NetSocketGenerator.Internal;

/// <summary>
/// Represents a connection over sockets, providing duplex communication
/// through <see cref="PipeReader"/> and <see cref="PipeWriter"/>. This class
/// handles sending and receiving data through the underlying <see cref="Socket"/>
/// with support for asynchronous resource management.
/// </summary>
/// <remarks>
/// The <see cref="SocketConnection"/> class internally manages separate sender and
/// receiver components using <see cref="SocketSender"/> and <see cref="SocketReceiver"/>.
/// It is designed for use with a provided memory buffer pool and a scheduler for efficient
/// handling of asynchronous operations.
/// </remarks>
internal sealed class SocketConnection : IDuplexPipe, IAsyncDisposable
{
   /// <summary>
   /// Gets the <see cref="PipeReader"/> instance used to receive incoming data
   /// through the socket connection. This provides a mechanism to asynchronously
   /// read data from the underlying transport layer.
   /// </summary>
   /// <remarks>
   /// The <see cref="Input"/> property represents the receiving half of the duplex
   /// pipe communication. Data read from this <see cref="PipeReader"/> originates
   /// from the socket, managed internally by the <see cref="SocketReceiver"/> component.
   /// </remarks>
   public PipeReader Input { get; }

   /// <summary>
   /// Gets the <see cref="PipeWriter"/> instance used to send outgoing data
   /// through the socket connection. This provides a mechanism to asynchronously
   /// write data to the underlying transport layer.
   /// </summary>
   /// <remarks>
   /// The <see cref="Output"/> property represents the sending half of the duplex
   /// pipe communication. Data written to this <see cref="PipeWriter"/> is transmitted
   /// to the remote endpoint, managed internally by the <see cref="SocketSender"/> component.
   /// </remarks>
   public PipeWriter Output { get; }

   /// <summary>
   /// Indicates whether the socket connection has been closed.
   /// This property is used to track the current open or closed state of the connection
   /// and is updated as part of the shutdown or cleanup process.
   /// </summary>
   /// <remarks>
   /// The <see cref="IsClosed"/> property is primarily managed internally to maintain
   /// the correct state of the connection lifecycle. Once set to true, it signifies that
   /// the socket connection is no longer active. External components or operations should
   /// validate this property to ensure actions are performed only on an open connection.
   /// </remarks>
   public bool IsClosed { get; set; }

   /// <summary>
   /// Gets or sets the exception that caused the connection to shut down.
   /// This property is set during the shutdown process if an error occurs
   /// or when the connection closes unexpectedly.
   /// </summary>
   /// <remarks>
   /// The <see cref="ShutdownException"/> property is used to store and expose
   /// the exception responsible for terminating the socket connection. It signifies
   /// an error or a graceful shutdown during the lifetime of the connection.
   /// A non-null value indicates that the connection has been closed due to an error
   /// or other specific conditions.
   /// Components such as <see cref="SocketReceiver"/> or <see cref="SocketConnection"/>
   /// rely on this property to identify the shutdown state and handle associated processing
   /// tasks, including resource cleanup or notifying dependent entities.
   /// </remarks>
   public Exception? ShutdownException { get; set; }

   /// <summary>
   /// Gets the <see cref="SocketReceiver"/> instance responsible for handling
   /// incoming data for the socket connection. This component manages the receipt
   /// of data in a non-blocking, asynchronous manner, providing it to the pipeline
   /// for further processing.
   /// </summary>
   /// <remarks>
   /// The <see cref="Receiver"/> property is a critical part of the duplex communication
   /// model. It ensures efficient reading from the underlying socket, leveraging
   /// the pipeline infrastructure to enable high-performance data streaming.
   /// The receiver operates independently of the sender, allowing bidirectional
   /// communication over the socket.
   /// </remarks>
   public SocketReceiver Receiver { get; private set; }

   /// <summary>
   /// Gets the <see cref="SocketSender"/> instance responsible for sending data
   /// through the underlying <see cref="Socket"/> associated with this connection.
   /// This component provides the sending half of the duplex communication.
   /// </summary>
   /// <remarks>
   /// The <see cref="Sender"/> property encapsulates the logic for sending data
   /// asynchronously using a <see cref="Pipe"/> for efficient buffering and flow
   /// control. It is internally managed as part of the <see cref="SocketConnection"/> lifecycle.
   /// </remarks>
   public SocketSender Sender { get; private set; }

   /// <summary>
   /// A private synchronization object used to ensure that operations related to
   /// shutting down the socket connection are thread-safe. This lock prevents
   /// concurrent access to critical sections during the shutdown process.
   /// </summary>
   /// <remarks>
   /// The <see cref="_shutdownLock"/> is utilized internally by the
   /// <see cref="SocketConnection"/> class to manage coordinated access
   /// when terminating the connection and releasing associated resources.
   /// Proper handling minimizes potential race conditions or deadlocks
   /// during the closure of the socket transport.
   /// </remarks>
   private readonly Lock _shutdownLock = new();

   /// <summary>
   /// Represents the underlying <see cref="Socket"/> used by the <see cref="SocketConnection"/>
   /// to facilitate network communication. This socket serves as the low-level transport layer component
   /// for sending and receiving data.
   /// </summary>
   /// <remarks>
   /// The <c>_socket</c> instance is used internally by both the <see cref="SocketSender"/> and
   /// <see cref="SocketReceiver"/> components to provide duplex communication capabilities. It is
   /// initialized when the <see cref="SocketConnection"/> is constructed and is disposed during shutdown
   /// to release network resources.
   /// </remarks>
   private readonly Socket _socket;

   /// <summary>
   /// Indicates whether the connection has been shut down, ensuring that any shutdown operations
   /// are performed only once, even if invoked multiple times. This flag helps prevent redundant
   /// calls to the shutdown logic by marking the state of the connection.
   /// </summary>
   /// <remarks>
   /// The <c>_isShutdown</c> field is set to <see langword="true"/> when the <see cref="Shutdown"/>
   /// method is executed. It prevents re-execution of resource cleanup and socket shutdown
   /// procedures by signaling that the connection has already completed its shutdown sequence.
   /// This is typically accessed and modified within a thread-safe block to ensure correct behavior
   /// in concurrent scenarios.
   /// </remarks>
   private bool _isShutdown;

   /// <summary>
   /// Represents a bidirectional socket-based connection that facilitates asynchronous communication
   /// using pipe readers and writers for input and output streams.
   /// </summary>
   /// <remarks>
   /// This class integrates a receiver and a sender component, each managing the respective
   /// data flow directions (receiving or sending). It initializes these components with configurable
   /// options and provides access to their associated <see cref="PipeReader"/> and <see cref="PipeWriter"/>.
   /// The connection supports proper resource disposal and can handle connection shutdown with error management.
   /// </remarks>
   public SocketConnection(
      MemoryPool<byte> bufferPool,
      PipeScheduler scheduler,
      Socket socket,
      PipeOptions receivePipeOptions,
      PipeOptions sendPipeOptions)
   {
      _socket = socket;

      Sender = new SocketSender(this, bufferPool, scheduler, socket, sendPipeOptions);
      Receiver = new SocketReceiver(this, bufferPool, scheduler, socket, receivePipeOptions);

      Input = Receiver.Pipe.Reader;
      Output = Sender.Pipe.Writer;
   }

   /// <summary>
   /// Starts the socket connection by initializing both the sender and receiver components.
   /// This method ensures that the sending and receiving operations are initiated in a coordinated manner,
   /// allowing bidirectional communication over the socket.
   /// </summary>
   /// <remarks>
   /// The method invokes the <see cref="SocketReceiver.Start"/> and <see cref="SocketSender.Start"/> methods
   /// to begin the asynchronous operations for receiving and sending data.
   /// It assumes that the receiver and sender objects have been properly initialized
   /// prior to being started and that the connection is ready to handle data transmission.
   /// </remarks>
   public void Start()
   {
      Receiver.Start();
      Sender.Start();
   }

   /// <summary>
   /// Shuts down the socket connection and releases associated resources.
   /// This method ensures that the socket is closed, the underlying connection is marked as shutdown,
   /// and any related exceptions are recorded for further diagnostic purposes.
   /// </summary>
   /// <remarks>
   /// This method is thread-safe and ensures that the shutdown process executes only once, even if called multiple times.
   /// It gracefully handles exceptions that may occur during the socket closing and shutdown process.
   /// </remarks>
   internal void Shutdown()
   {
      lock (_shutdownLock)
      {
         if (_isShutdown)
         {
            return;
         }
         
         _isShutdown = true;
         ShutdownException ??= new Exception("The socket transport send loop was completed gracefully");

         try
         {
            _socket.Close(timeout: 0);
         }
         catch (Exception)
         {
            // ignored
         }
         
         try
         {
            _socket.Shutdown(SocketShutdown.Both);
         }
         catch (Exception)
         {
            // ignored
         }
         
         _socket?.Dispose();
      }
   }

   /// <summary>
   /// Asynchronously disposes the resources held by the <see cref="SocketConnection"/> instance.
   /// Ensures that both the <see cref="SocketSender"/> and <see cref="SocketReceiver"/> instances
   /// are properly disposed to release resources related to the underlying socket operations.
   /// </summary>
   /// <remarks>
   /// This method completes the cleanup of both receiving and sending tasks and ensures the proper
   /// disposal of all managed resources associated with the connection.
   /// </remarks>
   /// <returns>A <see cref="ValueTask"/> that completes when the dispose operation is finished.</returns>
   public async ValueTask DisposeAsync()
   {
      await Receiver.DisposeAsync();
      await Sender.DisposeAsync();
   }
}