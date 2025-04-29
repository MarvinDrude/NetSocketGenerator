
namespace NetSocketGenerator.Tcp;

/// <summary>
/// Represents a TCP server that manages incoming TCP connections and provides methods
/// to start and stop the server.
/// </summary>
/// <remarks>
/// The TcpServer class is designed to handle TCP connections using configurable options.
/// It manages the lifecycle of the server, including binding to an endpoint,
/// handling multiple connections, and providing a secure or insecure communication layer.
/// </remarks>
public sealed class TcpServer : ITcpServer
{
   public TcpConnectionType ConnectionType { get; }

   private readonly ConcurrentDictionary<Guid, TcpServerConnection> _connections = [];
   private CancellationTokenSource? _runTokenSource;
   
   private readonly IConnectionFactory _connectionFactory;
   internal readonly ITcpFrameFactory FrameFactory;
   
   private Socket? _socket;

   private readonly EndPoint _endPoint;
   internal readonly TcpServerOptions Options;
   
   private readonly ServerFrameDispatcher _frameDispatcher = new();
   private readonly TcpServerConnectionGrouping _grouping = new();
   
   public TcpServer(TcpServerOptions options)
   {
      if (options is { IsSecure: true, Certificate: null })
      {
         throw new ArgumentException("Certificate is required for secure server connections");
      }

      if (!IPAddress.TryParse(options.Address, out var address))
      {
         throw new ArgumentException("Invalid address specified");
      }
      
      if (options.Port is < IPEndPoint.MinPort or > IPEndPoint.MaxPort)
      {
         throw new ArgumentException("Invalid port specified");
      }
      
      Options = options;
      _endPoint = new IPEndPoint(address, options.Port);
      
      ConnectionType = DetermineConnectionType(options);
      
      _connectionFactory = CreateFactory(ConnectionType, options);
      FrameFactory = new TcpFrameFactory();
   }

   /// <summary>
   /// Registers a message handler for a specific key to process incoming frame messages.
   /// </summary>
   /// <remarks>
   /// This method allows the caller to map a specific key to a custom handler that processes
   /// the corresponding frame messages. The registered handler will be invoked whenever a message
   /// with the specified key is received.
   /// </remarks>
   /// <param name="key">
   /// A string that represents the key associated with the message to be handled. This key
   /// is used to identify which handler to invoke for incoming messages.
   /// </param>
   /// <param name="handler">
   /// A delegate of type <see cref="ServerFrameMessageHandler"/> that processes the incoming
   /// connection, message identifier, and payload for messages associated with the specified key.
   /// </param>
   public void AddHandler(string key, ServerFrameMessageHandler handler)
   {
      if (_runTokenSource is not null)
      {
         throw new InvalidOperationException("Cannot add handlers while the server is running.");
      }
      
      _frameDispatcher.AddKeyHandler(key, handler);
   }

   /// <summary>
   /// Registers a raw message handler to process incoming frame messages.
   /// </summary>
   /// <remarks>
   /// This method allows the caller to add a custom handler that receives and processes
   /// the raw frame data from incoming TCP connections. The handler will be invoked for
   /// each message received.
   /// </remarks>
   /// <param name="handler">
   /// A delegate of type <see cref="ServerFrameMessageHandler"/> that processes the incoming
   /// connection, message identifier, and payload.
   /// </param>
   public void AddRawHandler(ServerFrameMessageHandler handler)
   {
      if (_runTokenSource is not null)
      {
         throw new InvalidOperationException("Cannot add handlers while the server is running.");
      }
      
      _frameDispatcher.AddRawHandler(handler);
   }
   
   /// <summary>
   /// Starts the TCP server, binding it to the configured endpoint and initiating the loop to accept incoming connections.
   /// </summary>
   /// <remarks>
   /// This method initializes the server and begins the process of listening for and accepting incoming TCP connections.
   /// If the server is already running, it does nothing. Internally, it binds the listening socket and uses a dedicated task
   /// to manage incoming connection handling in a loop.
   /// </remarks>
   public void Start()
   {
      if (_runTokenSource is not null)
      {
         return;
      }

      _connections.Clear();
      _grouping.Clear();
      
      _runTokenSource = new CancellationTokenSource();
      var token = _runTokenSource.Token;
      
      BindListenSocket();
      
      _ = Task.Factory.StartNew(
         async () => await RunAccept(token), 
         token, 
         TaskCreationOptions.LongRunning, 
         TaskScheduler.Default);
   }

   /// <summary>
   /// Stops the TCP server, releasing all resources associated with it and terminating any ongoing operations.
   /// </summary>
   /// <remarks>
   /// This method halts the server by canceling the accept loop and shutting down any active connections.
   /// It clears the internal connection dictionary, cancels the ongoing cancellation token, and disposes of the main socket.
   /// If the server is not running, the method simply exits. Upon stopping, the server can be restarted if needed by calling the Start method.
   /// </remarks>
   public async Task Stop()
   {
      if (_runTokenSource is null)
      {
         return;
      }
      
      _connections.Clear();
      _grouping.Clear();
      
      await _runTokenSource.CancelAsync();
      _runTokenSource.Dispose();
      
      _runTokenSource = null;

      foreach (var connection in _connections.Values)
      {
         await connection.DisposeAsync();
      }
      
      try
      {
         _socket?.Shutdown(SocketShutdown.Both);
      }
      catch (Exception) { /* ignore */ }

      _socket?.Dispose();
      _socket = null;
   }

   public void AddToGroup(string groupName, ITcpConnection connection)
   {
      AddToGroup(groupName, (connection as TcpServerConnection) ?? throw new ArgumentException("Invalid connection type"));;
   }

   public void RemoveFromGroup(string groupName, ITcpConnection connection)
   {
      RemoveFromGroup(groupName, (connection as TcpServerConnection) ?? throw new ArgumentException("Invalid connection type"));;
   }

   internal void AddToGroup(string groupName, TcpServerConnection connection)
   {
      _grouping.AddToGroup(groupName, connection);
   }

   internal void RemoveFromGroup(string groupName, TcpServerConnection connection)
   {
      _grouping.RemoveFromGroup(groupName, connection);
   }

   /// <summary>
   /// Continuously accepts incoming TCP connections and initiates the receive loop for each connection until the operation is canceled.
   /// </summary>
   /// <param name="token">A cancellation token that is used to stop the accept operation.</param>
   /// <returns>A task that represents the asynchronous accept operation.</returns>
   /// <remarks>
   /// This method listens for incoming TCP connections in a loop. For each accepted connection, it starts a receive operation.
   /// If the cancellation token is triggered, the accept loop will stop.
   /// </remarks>
   private async Task RunAccept(CancellationToken token)
   {
      while (!token.IsCancellationRequested)
      {
         if (await Accept(token) is not { } connection)
         {
            continue;
         }

         _connections.TryAdd(connection.Id, connection);
         _ = RunReceive(connection, token);
      }
   }

   /// <summary>
   /// Handles the process of receiving data from a TCP connection. This method reads incoming data,
   /// processes it into logical frames, and dispatches those frames for further handling.
   /// </summary>
   /// <param name="connection">The TCP connection instance from which data will be received.</param>
   /// <param name="token">A cancellation token used to observe cancellation requests for the receive loop.</param>
   /// <returns>An asynchronous task representing the receive operation.</returns>
   private async Task RunReceive(TcpServerConnection connection, CancellationToken token)
   {
      if (ConnectionType is TcpConnectionType.FastSocket)
      {
         CreateSocketConnection(connection);
      }
      else
      {
         await CreateStreamConnection(connection);
      }

      _ = RunSend(connection, token);
      await OnConnected(connection);
      
      var frame = FrameFactory.Create();
      
      try
      {
         while (!token.IsCancellationRequested 
                && !connection.DisconnectTokenSource.IsCancellationRequested
                && connection.Pipe is not null)
         {
            var result = await connection.Pipe.Input.ReadAsync(connection.DisconnectTokenSource.Token);
            var buffer = result.Buffer;
            var position = frame.Read(ref buffer);

            if (!frame.IsComplete)
            {
               continue;
            }

            await OnFrameReceived(connection, frame);
            await _frameDispatcher.Dispatch(frame, connection);
            
            connection.Pipe.Input.AdvanceTo(position);
            
            frame.Dispose();
            frame = FrameFactory.Create();
         }
      }
      finally
      {
         frame?.Dispose();
         await OnDisconnected(connection);
      }
   }

   /// <summary>
   /// Handles sending data from the server to a connected client through the specified connection.
   /// </summary>
   /// <remarks>
   /// This method continuously processes and sends frames from the connection's send channel
   /// to the underlying pipe until the operation is canceled or the connection is terminated.
   /// </remarks>
   /// <param name="connection">The TCP server connection representing the client to which data will be sent.</param>
   /// <param name="token">The cancellation token used to abort the sending operation if required.</param>
   /// <returns>A task that represents the asynchronous operation.</returns>
   private async Task RunSend(TcpServerConnection connection, CancellationToken token)
   {
      const int maxPerBatch = 1_000_000;
      
      while (!token.IsCancellationRequested
             && await connection.SendChannel.Reader.WaitToReadAsync(token)
             && connection.Pipe is not null)
      {
         try
         {
            var count = 0;
            
            while (connection.SendChannel.Reader.TryRead(out var frame)
                   && count++ < maxPerBatch)
            {
               if (!frame.IsForSending || frame.Data.Length == 0)
               {
                  continue;
               }

               frame.Send(connection.Pipe);
            }

            await connection.Pipe.Output.FlushAsync(token);
         }
         catch (Exception) { /* ignored */ }
      }
   }

   /// <summary>
   /// Binds the listen socket to the configured endpoint and sets up the socket for incoming connections.
   /// </summary>
   /// <exception cref="SocketException">
   /// Thrown if an error occurs while binding or setting up the socket.
   /// </exception>
   /// <remarks>
   /// This method initializes the server's listening socket with IPv6 support and disables the IPv6-only mode.
   /// It configures the socket to optimize performance for TCP connections and begins listening for incoming connections.
   /// </remarks>
   private void BindListenSocket()
   {
      Socket socket = new(AddressFamily.InterNetworkV6, SocketType.Stream, ProtocolType.Tcp);
      
      socket.SetSocketOption(SocketOptionLevel.IPv6, SocketOptionName.IPv6Only, false);
      socket.NoDelay = true;
      
      socket.Bind(_endPoint);
      
      _socket = socket;
      _socket.Listen();
   }

   /// <summary>
   /// Accepts an incoming TCP connection and creates a <c>TcpServerConnection</c> instance based on the configured connection type.
   /// </summary>
   /// <param name="token">
   /// A <c>CancellationToken</c> that can be used to cancel the accept operation.
   /// </param>
   /// <returns>
   /// A <c>ValueTask</c> that resolves to a <c>TcpServerConnection</c> object representing the accepted connection,
   /// or <c>null</c> if the operation is cancelled or an error occurs.
   /// </returns>
   /// <exception cref="InvalidOperationException">
   /// Thrown if the connection type is invalid.
   /// </exception>
   private async ValueTask<TcpServerConnection?> Accept(CancellationToken token)
   {
      while (!token.IsCancellationRequested && _socket is not null)
      {
         try
         {
            var socket = await _socket.AcceptAsync(token);
            socket.NoDelay = true;

            return new TcpServerConnection()
            {
               Id = Guid.CreateVersion7(),
               Socket = socket,
               Server = this,
            };
         }
         catch (ObjectDisposedException)
         {
            return null;
         }
         catch (SocketException e)
            when (e.SocketErrorCode == SocketError.OperationAborted)
         {
            return null;
         }
         catch (Exception)
         {
            // connection reset while backlog, try again  
         }
      }

      return null;
   }

   /// <summary>
   /// Sets up the connection by creating a pipe for the provided socket connection
   /// using the appropriate connection factory.
   /// </summary>
   /// <param name="connection">The TCP server connection to be configured. The connection should include a valid socket instance.</param>
   private void CreateSocketConnection(TcpServerConnection connection)
   {
      connection.Pipe = _connectionFactory.Create(connection.Socket, null);
   }

   /// <summary>
   /// Establishes a stream-based connection for the provided <see cref="TcpServerConnection"/> by creating
   /// a data stream from the associated socket and initializing a duplex pipe for communication.
   /// </summary>
   /// <param name="connection">
   /// The <see cref="TcpServerConnection"/> instance for which the stream-based connection will be created.
   /// This object must contain a valid socket.
   /// </param>
   /// <returns>
   /// A <see cref="Task{TResult}"/> that resolves to a boolean value indicating whether the connection was
   /// successfully established. Returns <c>true</c> when both the stream and pipe are created successfully;
   /// otherwise, <c>false</c>.
   /// </returns>
   private async Task<bool> CreateStreamConnection(TcpServerConnection connection)
   {
      if (await GetSocketStream(connection.Socket) is not { } stream)
      {
         return false;
      }

      connection.Pipe = _connectionFactory.Create(connection.Socket, stream);
      connection.Stream = stream;
      
      return true;
   }

   /// <summary>
   /// Creates a stream from the provided socket based on the configured <c>TcpConnectionType</c>.
   /// Returns a <c>NetworkStream</c> or <c>SslStream</c> depending on the connection type.
   /// If an error occurs during SSL authentication or the connection type is unsupported, it returns <c>null</c>.
   /// </summary>
   /// <param name="socket">
   /// The <c>Socket</c> instance from which to create the stream.
   /// </param>
   /// <returns>
   /// A <c>Task</c> that resolves to a <c>Stream</c> object representing the created socket stream,
   /// or <c>null</c> if the operation fails.
   /// </returns>
   private async Task<Stream?> GetSocketStream(Socket socket)
   {
      NetworkStream stream = new(socket);

      if(ConnectionType is TcpConnectionType.NetworkStream) 
      {
         return stream;
      }

      SslStream? sslStream = null;

      try
      {
         sslStream = new SslStream(stream, false);

         var task = sslStream.AuthenticateAsServerAsync(
            Options.Certificate!, false, SslProtocols.None, true);
         await task.WaitAsync(TimeSpan.FromSeconds(20));

         return sslStream;
      }
      catch (Exception)
      {
         if (sslStream is not null)
         {
            await sslStream.DisposeAsync();
         }

         return null;
      }
   }

   /// <summary>
   /// Invokes the configured event handler for a newly connected TCP client.
   /// </summary>
   /// <param name="connection">The connection object representing the newly established TCP connection.</param>
   /// <returns>A task that represents the asynchronous operation of invoking the event handler.</returns>
   /// <remarks>
   /// This method is called when a new TCP client successfully connects to the server.
   /// It executes the user-provided callback specified in <see cref="TcpOptions.Events.OnConnected"/>, if defined.
   /// </remarks>
   private async Task OnConnected(TcpServerConnection connection)
   {
      if (Options.Events.OnConnected is not null)
      {
         await Options.Events.OnConnected(connection);
      }
   }

   /// <summary>
   /// Handles the disconnection of a TCP server connection and performs cleanup operations.
   /// </summary>
   /// <param name="connection">The <see cref="TcpServerConnection"/> instance that has been disconnected.</param>
   /// <returns>A task representing the asynchronous operation.</returns>
   /// <remarks>
   /// This method ensures the proper disposal of the disconnected connection, removes the connection
   /// from the internal collection of active connections, and invokes the user-defined `OnDisconnected`
   /// callback, if provided, to handle additional custom logic upon disconnection.
   /// </remarks>
   private async Task OnDisconnected(TcpServerConnection connection)
   {
      await connection.DisposeAsync();

      if (!_connections.TryRemove(connection.Id, out _))
      {
         return;
      }
      
      if (Options.Events.OnDisconnected is not null)
      {
         await Options.Events.OnDisconnected(connection);
      }
   }

   /// <summary>
   /// Invokes the configured callback, if present, when a frame is received on a TCP connection.
   /// </summary>
   /// <param name="connection">The connection on which the frame was received.</param>
   /// <param name="frame">The frame that was received.</param>
   /// <returns>A task representing the asynchronous operation.</returns>
   private async Task OnFrameReceived(TcpServerConnection connection, ITcpFrame frame)
   {
      if (Options.Events.OnFrameReceived is not null)
      {
         await Options.Events.OnFrameReceived(connection, frame);
      }
   }

   /// <summary>
   /// Determines the type of TCP connection to use based on the provided options.
   /// </summary>
   /// <param name="options">
   /// An object containing the properties used to define TCP connection settings, such as security and preset connection type attributes.
   /// </param>
   /// <returns>
   /// A <see cref="TcpConnectionType"/> value representing the type of TCP connection to be established.
   /// </returns>
   internal static TcpConnectionType DetermineConnectionType(TcpOptions options)
   {
      return options.ConnectionType switch
      {
         TcpConnectionType.Unset when options.IsSecure is false => TcpConnectionType.FastSocket,
         TcpConnectionType.Unset when options.IsSecure => TcpConnectionType.SslStream,
         
         TcpConnectionType.FastSocket => TcpConnectionType.FastSocket,
         
         TcpConnectionType.SslStream when options.IsSecure => TcpConnectionType.SslStream,
         TcpConnectionType.NetworkStream when options.IsSecure is false => TcpConnectionType.NetworkStream,
         
         _ => TcpConnectionType.FastSocket
      };
   }

   /// <summary>
   /// Creates and returns an appropriate connection factory based on the specified connection type and options.
   /// </summary>
   /// <param name="connectionType">The type of connection to be established, which determines the specific factory to create.</param>
   /// <param name="options">The configuration options required to initialize the connection factory.</param>
   /// <returns>An instance of <see cref="IConnectionFactory"/> corresponding to the provided connection type.</returns>
   /// <exception cref="ArgumentException">Thrown when an invalid or unsupported connection type is specified.</exception>
   internal static IConnectionFactory CreateFactory(TcpConnectionType connectionType, TcpOptions options)
   {
      return connectionType switch
      {
         TcpConnectionType.FastSocket => new SocketConnectionFactory(options.SocketConnectionOptions),
         TcpConnectionType.NetworkStream or TcpConnectionType.SslStream => new StreamConnectionFactory(options.StreamConnectionOptions),
         _ => throw new ArgumentException("Invalid connection type specified")
      };
   }
}