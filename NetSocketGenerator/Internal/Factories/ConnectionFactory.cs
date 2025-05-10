namespace NetSocketGenerator.Internal.Factories;

/// <summary>
/// Represents an abstract factory for creating instances of connections that implement <see cref="IDuplexPipe"/>.
/// This class is designed to manage connection settings and provide specific connection implementations
/// based on the type parameters supplied.
/// </summary>
/// <typeparam name="TOptions">Specifies the type of options for connection configuration, inheriting from <see cref="ConnectionOptions"/>.</typeparam>
/// <typeparam name="TSettings">Defines the type of queue settings used for connection management, inheriting from <see cref="ConnectionQueueSettings"/>.</typeparam>
/// <typeparam name="TConnection">Represents the specific type of connection to be created, implementing <see cref="IDuplexPipe"/>.</typeparam>
internal abstract class ConnectionFactory<TOptions, TSettings, TConnection>
   : IConnectionFactory
   where TOptions : ConnectionOptions
   where TSettings : ConnectionQueueSettings
   where TConnection : IDuplexPipe
{
   /// <summary>
   /// Represents the configuration options used to customize the behavior and settings of connections.
   /// This field stores an instance of the type parameter <typeparamref name="TOptions"/>, which must
   /// derive from <see cref="ConnectionOptions"/>.
   /// </summary>
   /// <remarks>
   /// The _options field is initialized through the factory constructor and holds key connection
   /// configuration data, such as the number of I/O queues and any specific queue settings.
   /// It provides centralized access to connection parameters required for the creation
   /// and management of connections.
   /// </remarks>
   protected readonly TOptions _options;

   /// <summary>
   /// Represents the total number of connection queue settings that can be used for managing I/O operations.
   /// </summary>
   /// <remarks>
   /// The _settingsCount field is determined by the <c>IoQueueCount</c> property of the provided <typeparamref name="TOptions"/> instance.
   /// It is used to initialize and manage the array of <typeparamref name="TSettings"/> objects, ensuring that the appropriate
   /// number of settings is created and distributed across the connections.
   /// This field also plays a critical role in handling the round-robin allocation of settings during connection creation.
   /// </remarks>
   protected readonly int _settingsCount;

   protected readonly ulong _settingsCountAsLong;

   /// <summary>
   /// Holds an array of settings that define the configuration for individual connection queues.
   /// Each element in this array represents queue-specific settings as determined
   /// by the <typeparamref name="TSettings"/> type.
   /// </summary>
   /// <remarks>
   /// The _settings field is initialized based on the number of input/output queues defined
   /// by the <paramref name="IoQueueCount"/> property in the <typeparamref name="TOptions"/> instance.
   /// These settings are created during the factory construction and are reused to ensure efficient
   /// resource management across multiple connections.
   /// </remarks>
   private readonly TSettings[] _settings;

   /// <summary>
   /// Maintains an atomic index used for distributing connection settings
   /// across multiple connection queue configurations.
   /// This field serves as a counter that is incremented in a thread-safe
   /// manner to ensure proper index calculation for load-balancing logic.
   /// </summary>
   /// <remarks>
   /// The _settingsIndex field is primarily utilized during the process of
   /// creating connections in conjunction with the <see cref="_settingsCount"/>
   /// field. It helps in determining the appropriate setting to use by leveraging
   /// modular arithmetic for cyclic distribution among available settings.
   /// This mechanism supports scenarios with concurrent connection initialization
   /// by ensuring consistent and safe index updates.
   /// </remarks>
   protected ulong _settingsIndex;

   /// <summary>
   /// Indicates whether the resources used by the <see cref="ConnectionFactory{TOptions, TSettings, TConnection}"/>
   /// have been released and the object has been disposed of.
   /// </summary>
   /// <remarks>
   /// The _disposed field is a boolean flag that tracks the disposal state of the instance.
   /// It is set to <c>true</c> when the <see cref="ConnectionFactory{TOptions, TSettings, TConnection}.Dispose"/>
   /// method is invoked to release unmanaged resources and perform necessary cleanup.
   /// This field ensures that disposal logic is executed only once, preventing redundant operations or resource leaks.
   /// </remarks>
   private bool _disposed = false;

   /// <summary>
   /// An abstract factory class used to create and manage instances of connections, connection settings,
   /// and options required to establish or manage duplex connections.
   /// This class utilizes generics to allow for flexibility in defining the specific options, settings,
   /// and connection types that it manages.
   /// </summary>
   public ConnectionFactory(TOptions options)
   {
      _options = options;

      _settingsCount = options.IoQueueCount;
      _settingsCountAsLong = (ulong)_settingsCount;
      _settingsIndex = 0;

      _settings = new TSettings[_settingsCount];

      for (var e = 0; e < _settingsCount; e++)
      {
         _settings[e] = (TSettings)_options.CreateQueueSettings();
      }
   }

   public IDuplexPipe Create(Socket socket, Stream? optionalStream)
   {
      var setting = _settings[Interlocked.Increment(ref _settingsIndex) % _settingsCountAsLong];
      return CreateConnection(socket, optionalStream, setting);
   }

   /// <summary>
   /// Creates a new instance of a duplex connection using the provided socket, optional stream,
   /// and connection queue settings. This method must be implemented by derived classes to
   /// define the specific logic for creating the connection.
   /// </summary>
   /// <param name="socket">The socket instance used as the transport for the connection.</param>
   /// <param name="optionalStream">An optional stream that can be used alongside the socket
   /// for additional data exchange capabilities. Can be null.</param>
   /// <param name="settings">The connection queue settings that determine the configuration
   /// for the connection's behavior.</param>
   /// <returns>
   /// A new instance of the connection of type <typeparamref name="TConnection"/>, which is
   /// configured based on the input parameters.
   /// </returns>
   protected abstract TConnection CreateConnection(Socket socket, Stream? optionalStream, TSettings settings);

   /// <summary>
   /// Releases all resources used by the current instance of the <see cref="ConnectionFactory{TOptions, TSettings, TConnection}"/> class.
   /// </summary>
   /// <remarks>
   /// This method is responsible for cleaning up and releasing both managed and unmanaged resources associated with the
   /// factory. It ensures that all settings and related resources in the <see cref="ConnectionFactory{TOptions, TSettings, TConnection}"/>
   /// instance are properly disposed of to avoid resource leaks or dangling references.
   /// The method checks an internal disposal flag (<c>_disposed</c>) to ensure the operation is performed only once.
   /// Subsequent calls to this method will return immediately without performing any further disposal logic.
   /// </remarks>
   public void Dispose()
   {
      if (_disposed)
      {
         return;
      }
      
      _disposed = true;
      
      foreach (var setting in _settings)
      {
         setting.Dispose();
      }
   }
}