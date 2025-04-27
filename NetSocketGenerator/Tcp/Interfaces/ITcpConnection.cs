namespace NetSocketGenerator.Tcp.Interfaces;

public interface ITcpConnection
{
   public Guid Id { get; }

   /// <summary>
   /// Sends a frame of data over the active TCP connection. The frame type is determined by the generic type parameter.
   /// </summary>
   /// <typeparam name="TFrame">The type of the frame to send, which must implement <see cref="ITcpFrame"/> and have a parameterless constructor.</typeparam>
   /// <param name="identifier">The identifier associated with the frame being sent.</param>
   /// <param name="rawData">The raw data to encapsulate within the frame and send.</param>
   public void SendFrame<TFrame>(string identifier, string rawData)
      where TFrame : ITcpFrame, new();

   /// <summary>
   /// Sends a frame of data over the active TCP connection. The frame type is determined by the generic type parameter.
   /// </summary>
   /// <typeparam name="TFrame">The type of the frame to send, which must implement <see cref="ITcpFrame"/> and have a parameterless constructor.</typeparam>
   /// <param name="identifier">The identifier associated with the frame being sent.</param>
   /// <param name="rawData">The raw data to encapsulate within the frame and send.</param>
   public void SendFrame<TFrame>(string identifier, ReadOnlyMemory<byte> rawData)
      where TFrame : ITcpFrame, new();

   /// <summary>
   /// Sends a frame of data over the active TCP connection. The frame type is determined by the generic type parameter.
   /// </summary>
   /// <typeparam name="TFrame">The type of the frame to send, which must implement <see cref="ITcpFrame"/> and have a parameterless constructor.</typeparam>
   /// <param name="rawData">The raw data to encapsulate within the frame and send.</param>
   public void SendFrame<TFrame>(ReadOnlyMemory<byte> rawData)
      where TFrame : ITcpFrame, new();

   /// <summary>
   /// Sends data over the active TCP connection. The data can be sent either by specifying
   /// an identifier with the raw data or just raw data.
   /// </summary>
   public void Send(ITcpFrame frame);

   /// <summary>
   /// Terminates the active TCP connection and releases all associated resources.
   /// Ensures the cancellation of any ongoing operations and proper cleanup of internal components.
   /// May trigger a disconnection callback if one is configured.
   /// </summary>
   /// <returns>A task representing the asynchronous disconnect operation.</returns>
   public ValueTask Disconnect();
}