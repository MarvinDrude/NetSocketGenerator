namespace NetSocketGenerator.Tcp;

/// <summary>
/// Represents configuration options for establishing TCP-based connections.
/// </summary>
/// <remarks>
/// This class encapsulates various properties required for setting up a TCP connection, such as
/// the target address, port, security settings, handshake limits, and additional socket and stream options.
/// </remarks>
public class TcpOptions
{
   /// <summary>
   /// Gets the target address for the TCP connection.
   /// </summary>
   /// <remarks>
   /// The address can be specified as IP address,
   /// representing the endpoint to which the connection should be established.
   /// </remarks>
   public required string Address { get; init; }

   /// <summary>
   /// Gets the port number for the TCP connection.
   /// </summary>
   /// <remarks>
   /// Specifies the numerical port value used to establish the TCP connection with the target address.
   /// This property is required and must represent a valid port number (typically between 1 and 65535).
   /// </remarks>
   public required int Port { get; init; }

   /// <summary>
   /// Indicates whether the TCP connection is secured using encryption, such as TLS.
   /// </summary>
   /// <remarks>
   /// If set to true, the connection will use encryption to enhance security and protect data
   /// integrity and confidentiality during transmission. This typically involves leveraging
   /// protocols like TLS to secure the communication channel.
   /// </remarks>
   public bool IsSecure { get; init; }

   /// <summary>
   /// Gets or sets the configuration options specific to managing socket-based connections.
   /// </summary>
   /// <remarks>
   /// This property defines settings related to socket behavior, resource allocation, and pipe scheduling
   /// for connections, allowing for detailed control over underlying socket mechanisms.
   /// </remarks>
   public SocketConnectionOptions SocketConnectionOptions { get; init; } = new();

   /// <summary>
   /// Gets or sets the options for managing stream-based connections.
   /// </summary>
   /// <remarks>
   /// These options define behaviors and configurations specific to stream-oriented communication,
   /// allowing customization and management of connection properties tailored for stream-based scenarios.
   /// </remarks>
   public StreamConnectionOptions StreamConnectionOptions { get; init; } = new();

   /// <summary>
   /// Gets the type of TCP connection to be established.
   /// </summary>
   /// <remarks>
   /// Specifies the connection mechanism, such as FastSocket, NetworkStream, or SslStream,
   /// which determines how the underlying communication is handled.
   /// </remarks>
   public TcpConnectionType ConnectionType { get; init; } = TcpConnectionType.Unset;

   /// <summary>
   /// Gets the set of event callbacks used to handle TCP connection events.
   /// </summary>
   /// <remarks>
   /// The <c>Events</c> property contains callbacks for handling key TCP connection lifecycle events such as
   /// receiving a frame, connection establishment, and disconnection. These callbacks are invoked
   /// during the corresponding events and allow for custom behavior to be defined.
   /// </remarks>
   public TcpEventCallbacks Events { get; init; } = new();
}