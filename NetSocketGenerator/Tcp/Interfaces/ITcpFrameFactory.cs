namespace NetSocketGenerator.Tcp.Interfaces;

/// <summary>
/// Provides a factory for creating instances of <see cref="ITcpFrame"/>.
/// </summary>
public interface ITcpFrameFactory
{
   /// <summary>
   /// Creates and returns a new instance of an object implementing the <see cref="ITcpFrame"/> interface.
   /// </summary>
   /// <returns>An instance of <see cref="ITcpFrame"/> representing a TCP frame.</returns>
   public ITcpFrame Create();
}