namespace NetSocketGenerator.Enums;

/// <summary>
/// Represents the type of TCP connection that can be established.
/// </summary>
public enum TcpConnectionType
{
   /// <summary>
   /// Specifies that the TCP connection type is not set or defined.
   /// Typically used as a default value to indicate the absence of a specific connection type.
   /// </summary>
   Unset = 1,

   /// <summary>
   /// Represents a high-performance TCP connection type optimized for fast data transmission.
   /// This option is typically used for scenarios where low-latency communication is critical.
   /// </summary>
   FastSocket = 2,

   /// <summary>
   /// Specifies that the TCP connection type is based on a standard network stream.
   /// Used for handling raw data streams over TCP connections without additional abstractions or enhancements.
   /// </summary>
   NetworkStream = 3,

   /// <summary>
   /// Represents a TCP connection type that uses SSL/TLS for secure communication.
   /// This ensures encryption and authentication over the connection.
   /// </summary>
   SslStream = 4,
}