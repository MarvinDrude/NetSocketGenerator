namespace NetSocketGenerator.Tcp.Interfaces;

/// <summary>
/// Represents a TCP server connection that extends the functionality of a TCP connection.
/// </summary>
/// <remarks>
/// The ITcpServerConnection interface provides access to the server object associated with the connection,
/// allowing interaction with the underlying TCP server. It combines the features of a standard TCP connection
/// with additional server-related functionalities.
/// </remarks>
public interface ITcpServerConnection : ITcpConnection
{
   public ITcpServer CurrentServer { get; }
   
   public TcpServerConnectionGrouping Groups { get; }
   
   public void AddToGroup(string groupName);

   public void RemoveFromGroup(string groupName);
}