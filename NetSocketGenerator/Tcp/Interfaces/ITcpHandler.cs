namespace NetSocketGenerator.Tcp.Interfaces;

public interface ITcpHandler
{
   public string EventNamePattern { get; }
   
   public ServerFrameMessageHandler GetExecuteMethod(bool isServer);
}