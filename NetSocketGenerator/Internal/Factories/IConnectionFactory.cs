namespace NetSocketGenerator.Internal.Factories;

/// <summary>
/// Defines a factory interface for creating duplex pipe connections.
/// </summary>
internal interface IConnectionFactory : IDisposable
{
   /// <summary>
   /// Creates a new instance of an <see cref="IDuplexPipe"/> connection using the provided socket
   /// and optionally a stream, leveraging the underlying connection settings.
   /// </summary>
   /// <param name="socket">The socket used to establish the connection.</param>
   /// <param name="optionalStream">An optional stream to be associated with the connection, or null if not used.</param>
   /// <returns>A new instance of an <see cref="IDuplexPipe"/> representing the connection.</returns>
   public IDuplexPipe Create(Socket socket, Stream? optionalStream);
}