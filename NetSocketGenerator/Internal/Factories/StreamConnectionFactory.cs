namespace NetSocketGenerator.Internal.Factories;

/// <summary>
/// A factory class for creating instances of <see cref="StreamConnection"/>.
/// </summary>
/// <remarks>
/// The <see cref="StreamConnectionFactory"/> is responsible for managing the creation
/// of <see cref="StreamConnection"/> objects using the provided <see cref="StreamConnectionOptions"/>
/// and <see cref="StreamConnectionQueueSettings"/>. This factory extends the functionality of
/// <see cref="ConnectionFactory{TOptions, TSettings, TConnection}"/>.
/// </remarks>
internal sealed class StreamConnectionFactory
   : ConnectionFactory<StreamConnectionOptions, StreamConnectionQueueSettings, StreamConnection> {
   
   /// <summary>
   /// Factory for creating and managing <see cref="StreamConnection"/> instances.
   /// </summary>
   /// <remarks>
   /// The <see cref="StreamConnectionFactory"/> extends the base functionality of
   /// <see cref="ConnectionFactory{TOptions, TSettings, TConnection}"/> and provides
   /// implementation-specific logic for handling socket-based stream connections.
   /// It utilizes <see cref="StreamConnectionOptions"/> for configuration and
   /// <see cref="StreamConnectionQueueSettings"/> for control of connection queue behavior.
   /// </remarks>
   public StreamConnectionFactory(StreamConnectionOptions options)
      : base(options) 
   {

   }

   /// <summary>
   /// Creates an instance of <see cref="StreamConnection"/> using the specified parameters.
   /// </summary>
   /// <param name="socket">The network socket associated with the connection, used for context.</param>
   /// <param name="stream">The data stream for handling input and output operations. This parameter cannot be null.</param>
   /// <param name="settings">The configuration settings that determine the behavior of the connection, such as receive and send options.</param>
   /// <returns>A new instance of <see cref="StreamConnection"/> initialized with the provided stream and settings.</returns>
   protected override StreamConnection CreateConnection(
      Socket socket,
      Stream? stream,
      StreamConnectionQueueSettings settings)
   {
      ArgumentNullException.ThrowIfNull(stream, nameof(stream));
      
      return new StreamConnection(
         stream,
         settings.ReceiveOptions,
         settings.SendOptions);
   }

}