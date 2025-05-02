namespace NetSocketGenerator.Tcp.Interfaces;

public interface ITcpClient : ITcpConnection
{
   public void Connect();

   public void UseKeyHandler<T>()
      where T : ITcpHandler;
   
   public T GetMetadata<T>();
}