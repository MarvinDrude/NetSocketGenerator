
namespace NetSocketGenerator.Internal;

/// <summary>
/// Facilitates the transmission of data over a socket connection asynchronously.
/// This class utilizes a <see cref="Pipe"/> to manage the writing of outgoing data
/// and integrates with <see cref="Socket"/> for low-level network communication.
/// </summary>
internal sealed class SocketSender : IAsyncDisposable
{
   /// <summary>
   /// Represents a pipeline for managing asynchronous data transfers internally within the <see cref="SocketSender"/> class.
   /// This property facilitates asynchronous data streaming by connecting the data source and the destination socket,
   /// leveraging the System.IO.Pipelines infrastructure.
   /// </summary>
   /// <remarks>
   /// This pipeline is used specifically for buffering outgoing data that is transmitted via the associated socket
   /// connection. It provides efficient handling of memory and supports both synchronous and asynchronous operations,
   /// aiding in optimal performance for network communication.
   /// </remarks>
   public Pipe Pipe { get; }

   /// <summary>
   /// Represents the encapsulated connection instance utilized by <see cref="SocketSender"/> to facilitate
   /// bi-directional communication through the associated <see cref="SocketReceiver"/> and <see cref="SocketConnection"/>.
   /// </summary>
   /// <remarks>
   /// This variable acts as the primary link between the sender and receiver components of the socket communication framework.
   /// It ensures proper cleanup and error propagation during shutdowns or communication disruptions,
   /// maintaining synchronization across the pipeline operations of <see cref="SocketSender"/> and <see cref="SocketReceiver"/>.
   /// </remarks>
   private readonly SocketConnection _connection;

   /// <summary>
   /// Represents the network socket used for low-level communication within the <see cref="SocketSender"/> class.
   /// This field is responsible for sending data packets over the network as per the established connection.
   /// </summary>
   /// <remarks>
   /// This socket is initialized externally and provided to the <see cref="SocketSender"/> class
   /// during its construction. It facilitates asynchronous transmission of buffered data
   /// managed by the <see cref="Pipe"/> to the intended network endpoint.
   /// </remarks>
   private readonly Socket _socket;

   /// <summary>
   /// Represents the scheduler responsible for managing and optimizing the execution of asynchronous read and write operations
   /// within the <see cref="SocketSender"/> class.
   /// This scheduler is assigned to the internal pipe and helps to coordinate task execution for efficient data flow.
   /// </summary>
   /// <remarks>
   /// The scheduler is utilized to control how the data processes in the pipeline are scheduled.
   /// It ensures that resources are effectively allocated and that operations are executed in an orderly manner,
   /// aiming to improve the performance of network data transmission by optimizing task scheduling at the lower level.
   /// </remarks>
   private readonly PipeScheduler _scheduler;

   /// <summary>
   /// Represents the memory pool used for buffering outgoing data to optimize memory allocation and management during asynchronous socket communication.
   /// </summary>
   /// <remarks>
   /// This memory pool provides reusable byte buffers to minimize allocations and improve performance when sending data through the associated <see cref="Socket"/>.
   /// It is a key component in the efficient transmission pipeline, working in conjunction with <see cref="Pipe"/> and <see cref="SocketConnection"/>.
   /// </remarks>
   private readonly MemoryPool<byte> _bufferPool;

   /// <summary>
   /// Represents the task responsible for handling the asynchronous sending of data over the socket connection.
   /// </summary>
   /// <remarks>
   /// This task is initialized when the <see cref="SocketSender.Start"/> method is called and manages
   /// the lifecycle of data transmission utilizing the configured <see cref="Pipe"/> and <see cref="Socket"/>.
   /// It completes when all data has been successfully sent or if an error occurs, ensuring proper cleanup.
   /// </remarks>
   private Task? _sendTask;

   /// <summary>
   /// Responsible for managing the sending of data over a socket connection.
   /// This class leverages a <see cref="Pipe"/> for buffered data transmission and
   /// integrates with a <see cref="SocketConnection"/> to facilitate socket-based I/O operations.
   /// </summary>
   public SocketSender(
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
   /// Initiates the asynchronous send process by starting the main send loop.
   /// This method sets up the necessary task for data transmission via the
   /// configured <see cref="Pipe"/> and associated <see cref="Socket"/>.
   /// </summary>
   public void Start()
   {
      _sendTask = RunSend();
   }

   /// <summary>
   /// Executes the send loop responsible for reading data from the <see cref="Pipe"/> and sending it
   /// over the associated socket asynchronously. This method ensures that data is processed, forwarded,
   /// and any socket-related exceptions are handled properly, maintaining the integrity of the transmission process.
   /// </summary>
   /// <returns>A <see cref="Task"/> that represents the asynchronous operation of the send loop.</returns>
   private async Task RunSend()
   {
      Exception? error = null;

      try
      {
         while (true)
         {
            var result = await Pipe.Reader.ReadAsync();

            if (result.IsCanceled)
            {
               break;
            }

            var buffer = result.Buffer;

            if (!buffer.IsEmpty)
            {
               List<ArraySegment<byte>> bufferList = [];
               
               foreach (var memory in buffer)
               {
                  bufferList.Add(memory.GetArray());
               }
               
               var bytesSent = await _socket.SendAsync(bufferList);

               if (bytesSent == 0)
               {
                  // ignore
               }
            }
            
            Pipe.Reader.AdvanceTo(buffer.End);

            if (result.IsCompleted)
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
         _connection.Shutdown();
         
         try { await Pipe.Writer.CompleteAsync(error ?? _connection.ShutdownException); } catch { /*ignored*/ }
         try { await Pipe.Reader.CompleteAsync(error ?? _connection.ShutdownException); } catch { /*ignored*/ }
         
         _connection.Receiver.Pipe.Writer.CancelPendingFlush();
      }
   }

   /// <summary>
   /// Disposes the resources used by the current instance asynchronously. This method signals
   /// the completion of the <see cref="Pipe"/> writer and reader, ensuring proper release
   /// of associated resources, and awaits the completion of any ongoing send task.
   /// </summary>
   /// <returns>A <see cref="ValueTask"/> that represents the asynchronous dispose operation.</returns>
   public async ValueTask DisposeAsync()
   {
      try
      {
         await Pipe.Writer.CompleteAsync(_connection.ShutdownException);
         await Pipe.Reader.CompleteAsync(_connection.ShutdownException);
      }
      catch (Exception)
      {
         // ignored
      }

      try
      {
         if (_sendTask is not null)
         {
            await _sendTask;
         }
      }
      catch (Exception)
      {
         // ignored
      }
   }
}