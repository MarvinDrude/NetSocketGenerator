
using System.Security.Authentication;

namespace NetSocketGenerator.Tcp;

public sealed class TcpServer
{
   public TcpConnectionType ConnectionType { get; }

   private readonly ConcurrentDictionary<Guid, TcpServerConnection> _connections = [];
   private CancellationTokenSource? _runTokenSource;
   
   private readonly IConnectionFactory _connectionFactory;
   private readonly ITcpFrameFactory _frameFactory;
   
   private Socket? _socket;

   private readonly EndPoint _endPoint;
   private readonly TcpServerOptions _options;
   
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
      
      _options = options;
      _endPoint = new IPEndPoint(address, options.Port);
      
      ConnectionType = DetermineConnectionType();
      
      _connectionFactory = CreateFactory(ConnectionType);
      _frameFactory = new TcpFrameFactory();
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
      
      _runTokenSource = new CancellationTokenSource();
      var token = _runTokenSource.Token;
      
      BindListenSocket();
      
      _ = Task.Factory.StartNew(
         async () => await RunAccept(token), 
         token, 
         TaskCreationOptions.LongRunning, 
         TaskScheduler.Default);
   }

   public void Stop()
   {
      if (_runTokenSource is null)
      {
         return;
      }
      
      _runTokenSource.Cancel();
      _runTokenSource.Dispose();
      _runTokenSource = null;
      
      
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

         _ = RunReceive(connection, token);
      }
   }

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
      var frame = _frameFactory.Create();
      
      try
      {
         while (!token.IsCancellationRequested 
                && connection.Pipe is not null)
         {
            var result = await connection.Pipe.Input.ReadAsync(token);
            var buffer = result.Buffer;
            var position = frame.Read(ref buffer);

            if (!frame.IsComplete)
            {
               continue;
            }

            
            
            frame.Dispose();
            frame = _frameFactory.Create();
         }
      }
      finally
      {
         frame?.Dispose();
         await connection.DisposeAsync();

         
      }
   }

   private async Task RunSend(TcpServerConnection connection, CancellationToken token)
   {
      
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
         catch (InvalidOperationException)
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
            _options.Certificate!, false, SslProtocols.None, true);
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
   /// Determines the type of TCP connection to be used based on the server configuration options.
   /// </summary>
   /// <returns>
   /// The appropriate <c>TcpConnectionType</c> value for the current server configuration.
   /// </returns>
   private TcpConnectionType DetermineConnectionType()
   {
      return _options.ConnectionType switch
      {
         TcpConnectionType.Unset when _options.IsSecure is false => TcpConnectionType.FastSocket,
         TcpConnectionType.Unset when _options.IsSecure => TcpConnectionType.SslStream,
         
         TcpConnectionType.FastSocket => TcpConnectionType.FastSocket,
         
         TcpConnectionType.SslStream when _options.IsSecure => TcpConnectionType.SslStream,
         TcpConnectionType.NetworkStream when _options.IsSecure is false => TcpConnectionType.NetworkStream,
         
         _ => TcpConnectionType.FastSocket
      };
   }

   /// <summary>
   /// Creates a connection factory based on the specified connection type.
   /// </summary>
   /// <param name="connectionType">The type of connection to create. Must be a valid <c>TcpConnectionType</c>.</param>
   /// <returns>An implementation of <c>IConnectionFactory</c> suitable for the specified connection type.</returns>
   /// <exception cref="ArgumentException">Thrown when the specified <paramref name="connectionType"/> is invalid.</exception>
   private IConnectionFactory CreateFactory(TcpConnectionType connectionType)
   {
      return connectionType switch
      {
         TcpConnectionType.FastSocket => new SocketConnectionFactory(_options.SocketConnectionOptions),
         TcpConnectionType.NetworkStream or TcpConnectionType.SslStream => new StreamConnectionFactory(_options.StreamConnectionOptions),
         _ => throw new ArgumentException("Invalid connection type specified")
      };
   }
}