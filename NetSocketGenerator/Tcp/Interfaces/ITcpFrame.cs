namespace NetSocketGenerator.Tcp.Interfaces;

/// <summary>
/// Represents a TCP frame interface, providing structure and functionality
/// for handling TCP frame data and operations.
/// </summary>
public interface ITcpFrame : IDisposable
{
   /// <summary>
   /// Gets or sets the Identifier for the TCP frame.
   /// This property is used to uniquely identify the frame
   /// within the context of TCP communication.
   /// </summary>
   public string? Identifier { get; set; }

   /// <summary>
   /// Gets or sets the binary data associated with the TCP frame.
   /// This property represents the payload contents of the frame
   /// and is used for communication or data operations within the TCP context.
   /// </summary>
   public ReadOnlyMemory<byte> Data { get; set; }

   /// <summary>
   /// Gets or sets a value indicating whether the TCP frame is treated as raw data only.
   /// This property determines if the frame bypasses higher-level processing
   /// and is handled strictly as raw byte content.
   /// </summary>
   public bool IsRawOnly { get; set; }

   /// <summary>
   /// Gets or sets a value indicating whether the TCP frame is intended for sending.
   /// This property distinguishes frames that are prepared for transmission
   /// from those that may be used for other operations, such as receiving or processing data.
   /// </summary>
   public bool IsForSending { get; set; }

   /// <summary>
   /// Gets or sets a value indicating whether the TCP frame is complete and ready to be read or sent
   /// </summary>
   public bool IsComplete { get; set; }

   /// <summary>
   /// Calculates and returns the size of the complete data within the TCP frame.
   /// </summary>
   public int GetRawSize();

   /// <summary>
   /// Reads data from the provided buffer and updates the current object's state.
   /// </summary>
   /// <param name="buffer">A reference to the sequence of bytes to read from.</param>
   /// <returns>A <see cref="SequencePosition"/> representing the position in the buffer after the read operation.</returns>
   public SequencePosition Read(ref ReadOnlySequence<byte> buffer);

   /// <summary>
   /// Writes data from the current TCP frame into the specified buffer.
   /// </summary>
   /// <param name="buffer">A reference to the span of bytes where the TCP frame data will be written to.</param>
   public void Write(ref Span<byte> buffer);

   /// <summary>
   /// Sends the current TCP frame data through the given duplex pipe.
   /// </summary>
   /// <param name="pipe">The duplex pipe used for transmitting the TCP frame data.</param>
   public void Send(IDuplexPipe pipe)
   {
      var binarySize = GetRawSize();

      if (binarySize <= TcpConstants.SafeStackBufferSize)
      {
         Span<byte> buffer = stackalloc byte[binarySize];
         Write(ref buffer);
         
         pipe.Output.Write(buffer);
         return;
      }
      
      var rented = ArrayPool<byte>.Shared.Rent(binarySize);

      try
      {
         var span = rented.AsSpan()[..binarySize];
         Write(ref span);

         pipe.Output.Write(span);
      }
      finally
      {
         ArrayPool<byte>.Shared.Return(rented);
      }
   }
}