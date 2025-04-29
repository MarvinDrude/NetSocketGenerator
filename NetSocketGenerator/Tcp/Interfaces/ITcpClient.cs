namespace NetSocketGenerator.Tcp.Interfaces;

public interface ITcpClient : ITcpConnection
{
   public void Connect();
}