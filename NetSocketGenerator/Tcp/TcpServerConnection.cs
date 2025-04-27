
namespace NetSocketGenerator.Tcp;

public sealed class TcpServerConnection 
   : ITcpConnection, IAsyncDisposable
{
   [MemberNotNullWhen(true, nameof(Pipe))]
   public bool IsInitialized => Pipe is not null;
   
   public required Guid Id { get; init; }
   
   public required Socket Socket { get; init; }
   
   public required TcpServer Server { get; init; }
   
   public Stream? Stream { get; set; }
   
   public IDuplexPipe? Pipe { get; set; }
   
   internal readonly Channel<ITcpFrame> SendChannel = Channel.CreateUnbounded<ITcpFrame>();
   internal readonly CancellationTokenSource DisconnectTokenSource = new();

   private bool _disposed;

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
      SendChannel.Writer.TryWrite(frame);
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