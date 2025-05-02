namespace NetSocketGenerator.Tcp.Interfaces;

public interface ITcpServer
{
   public TcpServerConnectionGrouping Groups { get; }
   
   public void Start();
   public Task Stop();
   
   public void AddToGroup(string groupName, ITcpConnection connection);

   public void RemoveFromGroup(string groupName, ITcpConnection connection);

   public void UseKeyHandler<T>()
      where T : ITcpHandler;

   public T GetMetadata<T>();

   public ITcpServerConnection? GetConnection(Guid id);
}