namespace NetSocketGenerator.Tcp;

/// <summary>
/// Provides configuration options specifically for a TCP client.
/// </summary>
/// <remarks>
/// This class extends <see cref="TcpOptions"/> and introduces additional properties
/// tailored for TCP client configuration, such as specifying the target host and
/// defining the reconnect behavior.
/// </remarks>
/// <seealso cref="TcpOptions"/>
public class TcpClientOptions : TcpOptions
{
   /// <summary>
   /// Needed for secure authentication
   /// </summary>
   public string? Host { get; init; }

   /// <summary>
   /// Specifies the interval between reconnection attempts in case of a connection failure.
   /// </summary>
   /// <remarks>
   /// This property defines the time duration to wait before trying to re-establish a TCP connection
   /// after a failed attempt. It can be used to configure retry logic in scenarios where connectivity
   /// may be intermittent or temporarily unavailable.
   /// </remarks>
   public TimeSpan ReconnectInterval { get; init; }
}