
namespace NetSocketGenerator.Internal;

/// <summary>
/// Responsible for managing and handling asynchronous data reception over a socket connection.
/// Uses a <see cref="Pipe"/> for buffering and processing incoming data, and a <see cref="Socket"/> for low-level communication.
/// </summary>
internal sealed class SocketReceiver : IAsyncDisposable
{
   /// <summary>
   /// Represents a data processing pipeline, implemented using <see cref="Pipe"/>,
   /// that buffers and processes incoming data received from the socket.
   /// </summary>
   /// <remarks>
   /// This instance of <see cref="Pipe"/> is configured with specific <see cref="PipeOptions"/>
   /// to manage the flow of data during socket communication. It is used to write incoming
   /// data from the <see cref="Socket"/> and subsequently read the buffered data for further processing.
   /// The pipeline serves as a key part in handling asynchronous communication effectively,
   /// ensuring the efficient management of data streams.
   /// </remarks>
   public Pipe Pipe { get; }

   /// <summary>
   /// Specifies the minimum buffer size to allocate for data reception in the socket communication layer.
   /// </summary>
   /// <remarks>
   /// This value determines the size of the memory buffer used when receiving data over the socket.
   /// It is calculated as half of the block size defined by the <see cref="PinnedBlockMemoryPool"/>.
   /// The buffer ensures efficient memory usage and can impact the performance of the data reception process.
   /// </remarks>
   private static readonly int MinAllocBufferSize = PinnedBlockMemoryPool.BlockSize / 2;

   /// <summary>
   /// Represents the current socket connection managed by the <see cref="SocketReceiver"/>.
   /// Encapsulates the communication state, including the handling of shutdown events and exceptions.
   /// </summary>
   /// <remarks>
   /// This connection is used as a reference to monitor the lifecycle of the socket operation.
   /// It is essential in determining operational state changes, such as shutdown or error conditions,
   /// and contributes to the control mechanism for halting or continuing data reception.
   /// </remarks>
   private readonly SocketConnection _connection;

   /// <summary>
   /// Represents the underlying socket used for low-level data communication.
   /// </summary>
   /// <remarks>
   /// This socket is used to facilitate asynchronous communication within the <see cref="SocketReceiver"/> class.
   /// It serves as the primary interface for sending and receiving data over the network connection.
   /// </remarks>
   private readonly Socket _socket;

   /// <summary>
   /// Represents the <see cref="PipeScheduler"/> used to control the scheduling of operations on the <see cref="Pipe"/>.
   /// </summary>
   /// <remarks>
   /// This scheduler dictates the execution context for asynchronous pipe operations, enabling fine-grained control over threading behavior.
   /// It can help optimize the performance and concurrency of the socket communication by managing the execution of pipe readers and writers.
   /// </remarks>
   private readonly PipeScheduler _scheduler;

   /// <summary>
   /// Represents the memory pool used for allocating buffers to manage data reception operations.
   /// </summary>
   /// <remarks>
   /// This memory pool is used to provide reusable blocks of memory for efficient handling
   /// of incoming data during socket communication. It minimizes memory allocation overhead
   /// by reusing allocated buffers, thereby improving performance and reducing garbage collection pressure.
   /// </remarks>
   private readonly MemoryPool<byte> _bufferPool;

   /// <summary>
   /// Represents the task responsible for asynchronous data reception on the socket connection.
   /// </summary>
   /// <remarks>
   /// This task is initiated when the socket receiver starts receiving data via the <see cref="Start"/> method.
   /// It executes the data reception logic encapsulated within the <c>RunReceive</c> method, which processes
   /// data using the configured <see cref="Pipe"/>. The task lifecycle is managed to ensure proper cleanup
   /// during disposal.
   /// </remarks>
   private Task? _receiveTask;

   /// <summary>
   /// Handles the reception of socket data asynchronously and manages the flow of
   /// incoming data through a pipeline. This class integrates with a socket
   /// connection and processes data using a memory pool and pipe scheduler.
   /// Primarily designed to facilitate efficient and scalable socket communication
   /// within the system.
   /// </summary>
   public SocketReceiver(
      SocketConnection connection,
      MemoryPool<byte> bufferPool,
      PipeScheduler scheduler,
      Socket socket,
      PipeOptions pipeOptions)
   {
      _connection = connection;
      _socket = socket;
      Pipe = new Pipe(pipeOptions);
      
      _scheduler = scheduler;
      _bufferPool = bufferPool;
   }

   /// <summary>
   /// Initiates the asynchronous data reception process over the socket connection.
   /// Assigns the data reception task to manage the reading and buffering of incoming data
   /// using the <see cref="RunReceive"/> method. This method prepares the receiver to start
   /// processing data and ensures the reception task is properly scheduled.
   /// </summary>
   public void Start()
   {
      _receiveTask = RunReceive();
   }

   /// <summary>
   /// Executes the main receive loop for handling asynchronous data reception over the socket.
   /// Continuously reads data into a <see cref="Pipe"/> buffer until a shutdown exception or termination
   /// condition is encountered.
   /// </summary>
   /// <returns>
   /// A task representing the asynchronous receive operation. The task completes when the receive loop
   /// terminates due to a shutdown exception, an aborted operation, or the end of the data stream.
   /// </returns>
   private async Task RunReceive()
   {
      Exception? error = null;

      try
      {
         while (_connection.ShutdownException is null)
         {
            var buffer = Pipe.Writer.GetMemory(MinAllocBufferSize);
            var bytesRead = await _socket.ReceiveAsync(buffer);

            if (IsToAbort() || bytesRead == 0)
            {
               break;
            }
            
            Pipe.Writer.Advance(bytesRead);
            
            var flushTask = Pipe.Writer.FlushAsync();
            var paused = !flushTask.IsCompleted;
            var result = await flushTask;

            if (result.IsCompleted || result.IsCanceled)
            {
               break;
            }
         }
      }
      catch (Exception er)
      {
         error = er;
         _connection.ShutdownException = er;
      }
      finally
      {
         await Pipe.Writer.CompleteAsync(error ?? _connection.ShutdownException);
         FireConnectionClosed();
      }
   }

   /// <summary>
   /// Determines whether the receive operation should be aborted based on the state of the connection.
   /// Checks if a shutdown exception has been encountered in the associated <see cref="SocketConnection"/>.
   /// </summary>
   /// <returns>
   /// A boolean value indicating whether the receive loop should terminate. Returns <c>true</c> if the operation
   /// should be aborted due to a shutdown exception; otherwise, <c>false</c>.
   /// </returns>
   private bool IsToAbort()
   {
      return _connection.ShutdownException is not null;
   }

   /// <summary>
   /// Notifies the system that the associated socket connection has been closed.
   /// This method ensures that the connection state is properly updated by marking
   /// the <see cref="SocketConnection.IsClosed"/> property as true. Subsequent
   /// invocations have no effect if the connection is already marked as closed.
   /// </summary>
   /// <remarks>
   /// This method is intended to maintain consistency in the management of the
   /// connection lifecycle and should be used as part of the cleanup or shutdown
   /// process. It prevents redundant operations by checking the current state of
   /// the connection before modifying its properties.
   /// </remarks>
   private void FireConnectionClosed()
   {
      if (_connection.IsClosed)
      {
         return;
      }
      
      _connection.IsClosed = true;

      // Not needed currently
      // ThreadPool.UnsafeQueueUserWorkItem(state =>
      //    {  
      //       
      //    },
      //    this,
      //    preferLocal: false);
   }

   /// <summary>
   /// Asynchronously disposes of the resources used by the <see cref="SocketReceiver"/> instance.
   /// Completes the pipe reader and writer and ensures any running receive tasks are properly awaited.
   /// </summary>
   /// <returns>A <see cref="ValueTask"/> that represents the asynchronous operation.</returns>
   public async ValueTask DisposeAsync()
   {
      try
      {
         await Pipe.Writer.CompleteAsync();
         await Pipe.Reader.CompleteAsync();
      }
      catch (Exception)
      {
         // ignored
      }

      try
      {
         if (_receiveTask is not null)
         {
            await _receiveTask;
         }
      }
      catch (Exception)
      {
         // ignored
      }
   }
}