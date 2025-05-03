
namespace NetSocketGenerator.Tcp;

public sealed class TcpServerConnection 
   : ITcpServerConnection, IAsyncDisposable
{
   [MemberNotNullWhen(true, nameof(Pipe))]
   public bool IsInitialized => Pipe is not null;
   
   public required Guid Id { get; init; }
   
   public required Socket Socket { get; init; }
   
   public required TcpServer Server { get; init; }
   
   public required TcpServerConnectionGrouping Groups { get; init; }
   
   public ITcpServer CurrentServer => Server;

   public ITcpSerializer Serializer => Server.Options.Serializer;
   
   public Stream? Stream { get; set; }
   
   public IDuplexPipe? Pipe { get; set; }
   
   internal readonly Channel<ITcpFrame> SendChannel = Channel.CreateBounded<ITcpFrame>(new BoundedChannelOptions(5_000)
   {
      FullMode = BoundedChannelFullMode.DropWrite
   });
   internal readonly CancellationTokenSource DisconnectTokenSource = new();

   private bool _disposed;

   public void AddToGroup(string groupName)
   {
      Server.AddToGroup(groupName, this);
   }

   public void RemoveFromGroup(string groupName)
   {
      Server.RemoveFromGroup(groupName, this);
   }

   public bool Send(string identifier, string rawData)
   {
      return Send(identifier, Encoding.UTF8.GetBytes(rawData));
   }
   
   public bool Send(string identifier, ReadOnlyMemory<byte> rawData)
   {
      var frame = Server.FrameFactory.Create();

      frame.Identifier = identifier;
      frame.IsForSending = true;
      frame.Data = rawData;
      
      return Send(frame);
   }
   
   public bool Send<T>(string identifier, T data)
   {
      var frame = Server.FrameFactory.Create();

      frame.Identifier = identifier;
      frame.IsForSending = true;
      frame.Data = Server.Options.Serializer.SerializeAsMemory(data);
      
      return Send(frame);
   }
   
   public bool SendFrame<TFrame>(string identifier, string rawData)
      where TFrame : ITcpFrame, new()
   {
      return Send(new TFrame()
      {
         Identifier = identifier,
         IsForSending = true,
         Data = Encoding.UTF8.GetBytes(rawData)
      });
   }
   
   public bool SendFrame<TFrame>(string identifier, ReadOnlyMemory<byte> rawData)
      where TFrame : ITcpFrame, new()
   {
      return Send(new TFrame()
      {
         Identifier = identifier,
         IsForSending = true,
         Data = rawData
      });
   }

   public bool SendFrame<TFrame>(ReadOnlyMemory<byte> rawData)
      where TFrame : ITcpFrame, new()
   {
      return Send(new TFrame()
      {
         IsRawOnly = true,
         IsForSending = true,
         Data = rawData
      });
   }
      
   public bool Send(ITcpFrame frame)
   {
      //Console.WriteLine("ServerConnection.Send: " + frame.Identifier);
      frame.IsForSending = true;
      return SendChannel.Writer.TryWrite(frame);
   }
   
   public ValueTask Disconnect()
   {
      if (!DisconnectTokenSource.IsCancellationRequested)
      {
         DisconnectTokenSource.Cancel();
      }
      
      return ValueTask.CompletedTask;
   }
   
   public async ValueTask DisposeAsync()
   {
      if (_disposed)
      {
         return;
      }
      
      _disposed = true;

      try
      {
         if (Pipe is SocketConnection socketConnection)
         {
            await socketConnection.DisposeAsync();
         }
         else
         {
            try
            {
               Socket.Shutdown(SocketShutdown.Both);
            }
            catch (Exception) { /* ignored */ }

            if (Stream is not null)
            {
               await Stream.DisposeAsync();
            }

            Socket?.Dispose();
         }
         
         SendChannel.Writer.TryComplete();
         
         if (!DisconnectTokenSource.IsCancellationRequested)
         {
            await DisconnectTokenSource.CancelAsync();
         }
         
         DisconnectTokenSource.Dispose();
      }
      catch (Exception) { /* ignored */ }
   }
}