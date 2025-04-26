namespace NetSocketGenerator.Tcp;

public sealed class TcpServerConnection : IAsyncDisposable
{
   [MemberNotNullWhen(true, nameof(Pipe))]
   public bool IsInitialized => Pipe is not null;
   
   public required Guid Id { get; init; }
   
   public required Socket Socket { get; init; }
   
   public required TcpServer Server { get; init; }
   
   public Stream? Stream { get; set; }
   
   public IDuplexPipe? Pipe { get; set; }
   
   public async ValueTask DisposeAsync()
   {
      
      
      if (Stream != null) await Stream.DisposeAsync();
   }
}