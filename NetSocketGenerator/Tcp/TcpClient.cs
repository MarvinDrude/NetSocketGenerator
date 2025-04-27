namespace NetSocketGenerator.Tcp;

/// <summary>
/// Represents a client for managing TCP connections.
/// </summary>
public sealed class TcpClient : ITcpConnection
{
   /// <summary>
   /// This is not the same id as the server has for this client
   /// </summary>
   public Guid Id { get; } = Guid.NewGuid();
   
   public TcpConnectionType ConnectionType { get; }
   public bool IsConnected { get; private set; }

   private readonly EndPoint _endPoint;
   private readonly TcpClientOptions _options;
   
   private readonly IConnectionFactory _connectionFactory;
   private readonly ITcpFrameFactory _frameFactory;
   
   private readonly ServerFrameDispatcher _frameDispatcher = new();
   
   private readonly Channel<ITcpFrame> _sendChannel = Channel.CreateUnbounded<ITcpFrame>();
   private CancellationTokenSource? _runTokenSource;
   
   private IDuplexPipe? _pipe;
   private Socket? _socket;
   private Stream? _stream;
   
   public TcpClient(TcpClientOptions options)
   {
      if (options is { IsSecure: true, Host: null })
      {
         throw new ArgumentException("Host is required for secure connections", nameof(options));
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
      ConnectionType = TcpServer.DetermineConnectionType(options);
      
      _endPoint = new IPEndPoint(address, options.Port);
      
      _connectionFactory = TcpServer.CreateFactory(ConnectionType, options);
      _frameFactory = new TcpFrameFactory();
   }

   public void Send(string identifier, string rawData)
   {
      Send(identifier, Encoding.UTF8.GetBytes(rawData));
   }
   
   public void Send(string identifier, ReadOnlyMemory<byte> rawData)
   {
      var frame = _frameFactory.Create();

      frame.Identifier = identifier;
      frame.IsForSending = true;
      frame.Data = rawData;
      
      Send(frame);
   }
   
   public void Send<T>(string identifier, T data)
   {
      var frame = _frameFactory.Create();

      frame.Identifier = identifier;
      frame.IsForSending = true;
      frame.Data = _options.Serializer.SerializeAsMemory(data);
      
      Send(frame);
   }
   
   public void SendFrame<TFrame>(string identifier, string rawData)
      where TFrame : ITcpFrame, new()
   {
      Send(new TFrame()
      {
         Identifier = identifier,
         IsForSending = true,
         Data = Encoding.UTF8.GetBytes(rawData)
      });
   }
   
   public void SendFrame<TFrame>(string identifier, ReadOnlyMemory<byte> rawData)
      where TFrame : ITcpFrame, new()
   {
      Send(new TFrame()
      {
         Identifier = identifier,
         IsForSending = true,
         Data = rawData
      });
   }

   public void SendFrame<TFrame>(ReadOnlyMemory<byte> rawData)
      where TFrame : ITcpFrame, new()
   {
      Send(new TFrame()
      {
         IsRawOnly = true,
         IsForSending = true,
         Data = rawData
      });
   }
      
   public void Send(ITcpFrame frame)
   {
      frame.IsForSending = true;
      _sendChannel.Writer.TryWrite(frame);
   }

   /// <summary>
   /// Establishes a connection using the configured options. If already connected,
   /// subsequent calls to this method have no effect. Prepares the necessary resources
   /// for the connection and initiates the internal process for handling data transmission.
   /// </summary>
   public void Connect()
   {
      if (_runTokenSource is not null)
      {
         return;
      }

      while (_sendChannel.Reader.TryRead(out _))
      {
         // just empty channel
      }
      
      _runTokenSource = new CancellationTokenSource();
      _ = RunConnect(_runTokenSource.Token);
   }

   /// <summary>
   /// Terminates the active TCP connection and releases all relevant resources.
   /// Cancels any ongoing operations and ensures thorough cleanup of internal components.
   /// If a disconnection callback is configured, it will be invoked after successful cleanup.
   /// </summary>
   /// <returns>A task that represents the asynchronous disconnect operation.</returns>
   public async ValueTask Disconnect()
   {
      if (_runTokenSource is null)
      {
         return;
      }

      IsConnected = false;
      
      try
      {
         await _runTokenSource.CancelAsync();
         _runTokenSource.Dispose();
      } catch (Exception) { /* ignore */ }
      
      while (_sendChannel.Reader.TryRead(out _))
      {
         // just empty channel
      }

      if (_pipe is SocketConnection connection)
      {
         await connection.DisposeAsync();
      }
      else if (_socket is not null)
      {
         try 
         {
            _socket?.Shutdown(SocketShutdown.Both);
         } catch (Exception) { /* ignore */ }

         if (_stream is not null)
         {
            await _stream.DisposeAsync();
            _stream = null;
         }
         
         try 
         {
            _socket?.Dispose();
            _socket = null;
            
         } catch (Exception) { /* ignore */ }
      }
      
      _runTokenSource = null;

      if (_options.Events.OnDisconnected is not null)
      {
         await _options.Events.OnDisconnected(this);
      }
   }

   /// <summary>
   /// Handles the internal process of disconnecting from the active TCP connection,
   /// ensuring resource cleanup, and automatically attempts to reconnect after a set interval.
   /// Combines both disconnection and reconnection logic to provide seamless recovery
   /// in case of connection disruptions or manual intervention.
   /// </summary>
   /// <returns>A task that represents the asynchronous operation of internal disconnection and reconnection.</returns>
   private async Task InternalDisconnect()
   {
      await Disconnect();
      await Task.Delay(_options.ReconnectInterval);

      Connect();
   }

   /// <summary>
   /// Manages the asynchronous connection process to a TCP server. Establishes the necessary
   /// socket, stream, and pipe connections while handling reconnection logic if the connection
   /// fails. Maintains a continuous connection cycle until explicitly canceled.
   /// </summary>
   /// <param name="token">The cancellation token used to monitor for cancellation requests,
   /// enabling the process to be stopped gracefully.</param>
   /// <returns>A task representing the asynchronous connection operation.</returns>
   private async Task RunConnect(CancellationToken token)
   {
      while (!token.IsCancellationRequested)
      {
         try
         {
            _socket = new Socket(AddressFamily.InterNetworkV6, SocketType.Stream, ProtocolType.Tcp);
            _socket.NoDelay = true;

            _socket.SetSocketOption(SocketOptionLevel.IPv6, SocketOptionName.IPv6Only, false);
            await _socket.ConnectAsync(_endPoint, token);

            if (ConnectionType is TcpConnectionType.FastSocket)
            {
               _pipe = _connectionFactory.Create(_socket, null);
            }
            else
            {
               _stream = await GetSocketStream(_socket);
               _pipe = _connectionFactory.Create(_socket, _stream);
            }

            _ = RunReceive(token);
            _ = RunSend(token);
            
            IsConnected = true;
            if (_options.Events.OnConnected is not null)
            {
               await _options.Events.OnConnected(this);
            }

            return;
         }
         catch (Exception) { /* ignore */ }

         await Task.Delay(_options.ReconnectInterval, token);
      }
   }

   /// <summary>
   /// Continuously receives data from the network connection, processes it using the configured frame factory,
   /// and dispatches frames to appropriate event handlers or processors. Handles cancellation requests
   /// and ensures proper resource cleanup in case of errors or disconnection.
   /// </summary>
   /// <param name="token">A <see cref="CancellationToken"/> that can be used to request cancellation of the receive loop.</param>
   /// <returns>A <see cref="Task"/> representing the asynchronous operation and facilitating error handling or awaiting completion.</returns>
   private async Task RunReceive(CancellationToken token)
   {
      var frame = _frameFactory.Create();

      try
      {
         while (!token.IsCancellationRequested 
                && _pipe is not null)
         {
            var result = await _pipe.Input.ReadAsync(token);
            var buffer = result.Buffer;
            var position = frame.Read(ref buffer);

            if (!frame.IsComplete)
            {
               continue;
            }

            if (_options.Events.OnFrameReceived is not null)
            {
               await _options.Events.OnFrameReceived(this, frame);
            }
            await _frameDispatcher.Dispatch(frame, this);
            
            _pipe.Input.AdvanceTo(position);

            if (result.IsCanceled || result.IsCompleted)
            {
               break;
            }
            
            frame.Dispose();
            frame = _frameFactory.Create();
         }
      }
      catch (Exception)
      {
         /* ignore */
      }
      finally
      {
         frame?.Dispose();
         await InternalDisconnect();
      }
   }

   /// <summary>
   /// Manages the asynchronous transmission of data frames through the TCP connection.
   /// Continuously reads from the send channel and forwards the data to the output pipe,
   /// ensuring proper data flow until the cancellation token is triggered or the connection is terminated.
   /// </summary>
   /// <param name="token">A cancellation token that is used to signal the method to stop execution.</param>
   /// <returns>A task representing the asynchronous execution of the send operation.</returns>
   private async Task RunSend(CancellationToken token)
   {
      while (!token.IsCancellationRequested
             && _pipe is not null
             && await _sendChannel.Reader.WaitToReadAsync(token))
      {
         try
         {
            var frame = await _sendChannel.Reader.ReadAsync(token);

            if (frame.Data.Length == 0)
            {
               continue;
            }

            frame.Send(_pipe);
            await _pipe.Output.FlushAsync(token);
         }
         catch (Exception er)
         {
             /* ignored */
             _ = er;
         }
      }
   }

   /// <summary>
   /// Retrieves a stream associated with the given socket, which can either be a
   /// <see cref="NetworkStream"/> or an <see cref="SslStream"/> based on the connection type.
   /// </summary>
   /// <param name="socket">The socket from which the stream is obtained.</param>
   /// <returns>
   /// A task that represents the asynchronous operation. The task result contains the
   /// <see cref="Stream"/> object. Returns null if an error occurs while creating the stream.
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

         var task = sslStream.AuthenticateAsClientAsync(_options.Host!, null, SslProtocols.None, true);
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
}