namespace NetSocketGenerator.Tcp;

public sealed class TcpClient : ITcpConnection
{
   public TcpConnectionType ConnectionType { get; }

   private readonly EndPoint _endPoint;
   private readonly TcpClientOptions _options;
   
   private readonly IConnectionFactory _connectionFactory;
   private readonly ITcpFrameFactory _frameFactory;
   
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

   public async Task Disconnect()
   {
      
   }

   private async Task RunConnect(CancellationToken token)
   {
      
   }
}