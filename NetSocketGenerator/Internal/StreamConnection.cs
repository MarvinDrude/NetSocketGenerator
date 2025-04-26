namespace NetSocketGenerator.Internal;

internal sealed class StreamConnection : IDuplexPipe
{
   /// <summary>
   /// Represents the minimum size of memory buffer allocation used for efficient data processing in the <see cref="StreamConnection"/>.
   /// </summary>
   /// <remarks>
   /// This field determines the size of the buffer memory blocks that are allocated during stream read operations.
   /// It is derived from half the block size managed by <see cref="PinnedBlockMemoryPool"/> to ensure optimal memory usage
   /// while maintaining alignment with the underlying memory pool's allocation strategy.
   /// </remarks>
   private static readonly int MinAllocBufferSize = PinnedBlockMemoryPool.BlockSize / 2;

   /// <summary>
   /// Gets the <see cref="PipeWriter"/> instance used for producing outgoing data in the <see cref="StreamConnection"/>.
   /// </summary>
   /// <remarks>
   /// This property provides access to the writing pipeline of the underlying stream, enabling efficient data production.
   /// It ensures proper handling of flow control, buffering, and concurrent write operations.
   /// </remarks>
   public PipeWriter Output => _writePipe.Writer;

   /// <summary>
   /// Gets the <see cref="PipeReader"/> instance used for consuming incoming data in the <see cref="StreamConnection"/>.
   /// </summary>
   /// <remarks>
   /// This property provides access to the reading pipeline of the underlying stream, enabling efficient data consumption.
   /// It ensures proper handling of flow control, buffering, and concurrent read operations.
   /// </remarks>
   public PipeReader Input => _readPipe.Reader;

   /// <summary>
   /// The <see cref="Pipe"/> instance used as the reading pipeline for the <see cref="StreamConnection"/>.
   /// </summary>
   /// <remarks>
   /// This pipeline is responsible for buffering and processing incoming data from the underlying stream efficiently.
   /// It supports concurrent reads while ensuring optimal flow control and backpressure mechanisms.
   /// </remarks>
   private readonly Pipe _readPipe;

   /// <summary>
   /// The <see cref="Pipe"/> instance used as the writing pipeline for the <see cref="StreamConnection"/>.
   /// </summary>
   /// <remarks>
   /// This pipeline is responsible for buffering and transferring outgoing data to the underlying stream efficiently.
   /// It supports concurrent writes while ensuring optimal flow control and backpressure mechanisms.
   /// </remarks>
   private readonly Pipe _writePipe;

   /// <summary>
   /// The underlying <see cref="Stream"/> used by the <see cref="StreamConnection"/> for reading and writing data.
   /// </summary>
   /// <remarks>
   /// This stream serves as the transport layer for data communication in the duplex connection.
   /// It must support both reading and writing operations. The stream is encapsulated by the
   /// pipeline system for efficient processing.
   /// </remarks>
   private readonly Stream _stream;

   /// <summary>
   /// Represents a duplex connection for reading from and writing to a stream using pipelines.
   /// </summary>
   /// <remarks>
   /// Provides pipe-based access to the underlying stream for producing and consuming data.
   /// </remarks>
   public StreamConnection(
      Stream stream,
      PipeOptions readOptions,
      PipeOptions writeOptions)
   {
      _readPipe = new Pipe(readOptions);
      _writePipe = new Pipe(writeOptions);

      _stream = stream;

      ArgumentOutOfRangeException.ThrowIfEqual(stream.CanWrite, false);
      ArgumentOutOfRangeException.ThrowIfEqual(stream.CanRead, false);
      
      writeOptions.WriterScheduler.Schedule(ob => ((StreamConnection)ob!).CopyWritePipeToStream().PipeFireAndForget(), this);
      readOptions.ReaderScheduler.Schedule(ob => ((StreamConnection)ob!).CopyStreamToReadPipe().PipeFireAndForget(), this);
   }

   /// <summary>
   /// Copies data from the write pipe to the underlying stream asynchronously.
   /// </summary>
   /// <returns>A task that represents the asynchronous operation of copying data from the write pipe to the stream.</returns>
   private async Task CopyWritePipeToStream()
   {
      var reader = _writePipe.Reader;

      try
      {
         while (true)
         {
            var pending = reader.ReadAsync();

            if (!pending.IsCompleted)
            {
               // nothing to do synchronously
               await _stream.FlushAsync(); // flush now then
            }

            var result = await pending;
            ReadOnlySequence<byte> buffer;

            do
            {
               buffer = result.Buffer;

               if (!buffer.IsEmpty)
               {
                  await SetBuffer(buffer);
               }

               reader.AdvanceTo(buffer.End);
               
            } while (!(buffer.IsEmpty && result.IsCompleted) && reader.TryRead(out result));

            if (buffer.IsEmpty && result.IsCompleted)
            {
               break;
            }

            if (result.IsCanceled)
            {
               break;
            }
         }

         await reader.CompleteAsync(null);
      }
      catch (Exception er)
      {
         try
         {
            await reader.CompleteAsync(er);
         }
         catch { /*ignored*/ }
      }
   }

   /// <summary>
   /// Copies data from the underlying stream to the reading pipe asynchronously.
   /// </summary>
   /// <returns>A task that represents the asynchronous operation of copying data from the stream to the pipe.</returns>
   private async Task CopyStreamToReadPipe()
   {
      Exception? error = null;
      var writer = _readPipe.Writer;

      try
      {
         while (true)
         {
            var memory = writer.GetMemory(MinAllocBufferSize);
            var read = await _stream.ReadAsync(memory);

            if (read <= 0)
            {
               break;
            }

            writer.Advance(read);

            var fres = await writer.FlushAsync();
            if (fres.IsCanceled || fres.IsCompleted)
            {
               break;
            }
         }
      }
      catch (Exception er)
      {
         error = er;
      }

      await writer.CompleteAsync(error);
   }

   /// <summary>
   /// Writes the provided read-only sequence of bytes to the underlying stream asynchronously.
   /// </summary>
   /// <param name="data">The read-only sequence of bytes to be written to the stream.</param>
   /// <returns>A task that represents the asynchronous write operation.</returns>
   private Task SetBuffer(in ReadOnlySequence<byte> data)
   {
      if (data.IsSingleSegment)
      {
         var vtask = _stream.WriteAsync(data.First);
         return vtask.IsCompletedSuccessfully ? Task.CompletedTask : vtask.AsTask();
      }
      else
      {
         return SetBufferSegments(data);
      }
   }

   /// <summary>
   /// Writes each segment from the provided read-only sequence of bytes to the underlying stream asynchronously.
   /// </summary>
   /// <param name="data">The read-only sequence of bytes to be written to the stream.</param>
   /// <returns>A task that represents the asynchronous write operation.</returns>
   private async Task SetBufferSegments(ReadOnlySequence<byte> data)
   {
      foreach (var segment in data)
      {
         await _stream.WriteAsync(segment);
      }
   }
}