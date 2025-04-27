namespace NetSocketGenerator.Tcp.Interfaces;

public interface ITcpConnection
{
   public Guid Id { get; }

   /// <summary>
   /// Sends data over the current TCP connection.
   /// </summary>
   /// <param name="identifier">The unique identifier for the data being sent.</param>
   /// <param name="rawData">The raw data to send as a string.</param>
   public bool Send(string identifier, string rawData);

   /// <summary>
   /// Sends data over the current TCP connection.
   /// </summary>
   /// <param name="identifier">The unique identifier for the data being sent.</param>
   /// <param name="rawData">The raw data to send as a read-only memory buffer.</param>
   public bool Send(string identifier, ReadOnlyMemory<byte> rawData);

   /// <summary>
   /// Sends data over the current TCP connection. The type of data is specified by the generic type parameter.
   /// This method serializes the data with the default serializer.
   /// </summary>
   /// <typeparam name="T">The type of data to send.</typeparam>
   /// <param name="identifier">The unique identifier for the data being sent.</param>
   /// <param name="data">The data to be serialized and then sent.</param>
   public bool Send<T>(string identifier, T data);

   /// <summary>
   /// Sends a frame of data over the active TCP connection. The frame type is determined by the generic type parameter.
   /// </summary>
   /// <typeparam name="TFrame">The type of the frame to send, which must implement <see cref="ITcpFrame"/> and have a parameterless constructor.</typeparam>
   /// <param name="identifier">The identifier associated with the frame being sent.</param>
   /// <param name="rawData">The raw data to encapsulate within the frame and send.</param>
   public bool SendFrame<TFrame>(string identifier, string rawData)
      where TFrame : ITcpFrame, new();

   /// <summary>
   /// Sends a frame of data over the active TCP connection. The frame type is determined by the generic type parameter.
   /// </summary>
   /// <typeparam name="TFrame">The type of the frame to send, which must implement <see cref="ITcpFrame"/> and have a parameterless constructor.</typeparam>
   /// <param name="identifier">The identifier associated with the frame being sent.</param>
   /// <param name="rawData">The raw data to encapsulate within the frame and send.</param>
   public bool SendFrame<TFrame>(string identifier, ReadOnlyMemory<byte> rawData)
      where TFrame : ITcpFrame, new();

   /// <summary>
   /// Sends a frame of data over the active TCP connection. The frame type is determined by the generic type parameter.
   /// </summary>
   /// <typeparam name="TFrame">The type of the frame to send, which must implement <see cref="ITcpFrame"/> and have a parameterless constructor.</typeparam>
   /// <param name="rawData">The raw data to encapsulate within the frame and send.</param>
   public bool SendFrame<TFrame>(ReadOnlyMemory<byte> rawData)
      where TFrame : ITcpFrame, new();

   /// <summary>
   /// Sends data over the active TCP connection. The data can be sent either by specifying
   /// an identifier with the raw data or just raw data.
   /// </summary>
   public bool Send(ITcpFrame frame);

   /// <summary>
   /// Terminates the active TCP connection and releases all associated resources.
   /// Ensures the cancellation of any ongoing operations and proper cleanup of internal components.
   /// May trigger a disconnection callback if one is configured.
   /// </summary>
   /// <returns>A task representing the asynchronous disconnect operation.</returns>
   public ValueTask Disconnect();
}