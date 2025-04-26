namespace NetSocketGenerator.Internal.Factories;

/// <summary>
/// Factory responsible for creating instances of <see cref="SocketConnection"/> using the specified options and settings.
/// </summary>
internal sealed class SocketConnectionFactory
   : ConnectionFactory<SocketConnectionOptions, SocketConnectionQueueSettings, SocketConnection> {
   
   /// <summary>
   /// Represents a factory for creating instances of <see cref="SocketConnection"/>
   /// based on the provided options and queue settings.
   /// </summary>
   /// <remarks>
   /// This factory integrates specialized options and queue settings to create
   /// and manage instances of <see cref="SocketConnection"/> efficiently.
   /// </remarks>
   public SocketConnectionFactory(SocketConnectionOptions options)
      : base(options) 
   {

   }

   /// <summary>
   /// Creates a new instance of <see cref="SocketConnection"/> using the provided socket, optional stream,
   /// and queue settings.
   /// </summary>
   /// <param name="socket">The underlying <see cref="Socket"/> used for communication.</param>
   /// <param name="stream">An optional <see cref="Stream"/> wrapping the socket for additional processing or transport.</param>
   /// <param name="settings">The <see cref="SocketConnectionQueueSettings"/> that configure connection behavior, including memory pooling and scheduling.</param>
   /// <returns>A new instance of <see cref="SocketConnection"/> configured with the specified parameters.</returns>
   protected override SocketConnection CreateConnection(
      Socket socket, 
      Stream? stream,
      SocketConnectionQueueSettings settings)
   {
      return new SocketConnection(
         settings.MemoryPool,
         settings.Scheduler,
         socket,
         settings.ReceiveOptions,
         settings.SendOptions);
   }

}