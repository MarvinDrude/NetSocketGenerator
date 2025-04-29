namespace NetSocketGenerator.Tcp;

/// <summary>
/// Provides mechanisms to group and manage multiple TCP server connections under named groups,
/// enabling operations such as adding, removing, and clearing connections within these groups.
/// </summary>
public sealed class TcpServerConnectionGrouping
{
   private static readonly ITcpConnection EmptyConnectionPlaceholder 
      = new TcpServerConnectionGroup() { Server = null! };
   
   private readonly ConcurrentDictionary<string, TcpServerConnectionGroup> _groups = [];

   public ITcpConnection this[string groupName] =>
      _groups.TryGetValue(groupName, out var group)
         ? group
         : EmptyConnectionPlaceholder;

   public void AddToGroup(string groupName, TcpServerConnection connection)
   {
      var group = _groups.GetOrAdd(groupName, _ => new TcpServerConnectionGroup()
      {
         Server = connection.Server
      });

      lock (group.Lock)
      {
         group.AddConnection(connection);
      }
   }

   public void RemoveFromGroup(string groupName, TcpServerConnection connection)
   {
      if (!_groups.TryGetValue(groupName, out var group))
      {
         return;
      }

      lock (group.Lock)
      {
         group.RemoveConnection(connection);

         var kvp = new KeyValuePair<string, TcpServerConnectionGroup>(groupName, group);
         if (group.ConnectionCount == 0)
         {
            _groups.TryRemove(kvp);
         }
      }
   }
   
   internal void Clear()
   {
      _groups.Clear();
   }
}

/// <summary>
/// Represents a group of TCP server connections, allowing management and sending of data
/// to multiple connections within the group.
/// </summary>
internal sealed class TcpServerConnectionGroup : ITcpConnection
{
   public Guid Id { get; } = Guid.CreateVersion7();
 
   internal Lock Lock { get; } = new();
   
   public required TcpServer Server { get; init; }
   
   public int ConnectionCount => _connections.Count;
   
   private readonly ConcurrentDictionary<Guid, TcpServerConnection> _connections = [];
   
   internal void AddConnection(TcpServerConnection connection)
   {
      _connections.TryAdd(connection.Id, connection);
   }

   internal void RemoveConnection(TcpServerConnection connection)
   {
      _connections.TryRemove(connection.Id, out _);
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
      if (_connections.IsEmpty)
      {
         return false;
      }
      
      foreach (var connection in _connections.Values)
      {
         connection.Send(frame);
      }

      return !_connections.IsEmpty;
   }

   public async ValueTask Disconnect()
   {
      if (_connections.IsEmpty)
      {
         return;
      }
      
      foreach (var connection in _connections.Values)
      {
         await connection.Disconnect();
      }
   }
}