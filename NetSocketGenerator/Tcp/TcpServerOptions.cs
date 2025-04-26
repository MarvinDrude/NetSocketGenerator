
namespace NetSocketGenerator.Tcp;

/// <summary>
/// Represents configuration options for a TCP server.
/// </summary>
/// <remarks>
/// This class extends <see cref="TcpOptions"/> by including additional settings specific to secure server configurations,
/// such as TLS/SSL certificates. It is used to define and customize the behavior of TCP servers.
/// </remarks>
public class TcpServerOptions : TcpOptions
{
   /// <summary>
   /// Gets the TLS/SSL certificate used to secure the TCP server connection.
   /// </summary>
   /// <remarks>
   /// This property specifies the X.509 certificate to be used for establishing secure connections
   /// in a TCP server. It is optional and is applicable only when <see cref="TcpOptions.IsSecure"/>
   /// is set to true. If null, the server will not use a certificate for TLS/SSL encryption.
   /// </remarks>
   public X509Certificate2? Certificate { get; init; }
}