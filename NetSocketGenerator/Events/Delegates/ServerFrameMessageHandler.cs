namespace NetSocketGenerator.Events.Delegates;

/// <summary>
/// Represents a delegate that handles server frame messages received over a TCP connection.
/// </summary>
/// <param name="connection">The TCP connection through which the server frame message is received.</param>
/// <param name="id">An optional identifier associated with the server frame message.</param>
/// <param name="payload">The payload data of the server frame message.</param>
/// <returns>A task that represents the asynchronous operation of handling the message.</returns>
public delegate Task ServerFrameMessageHandler(ITcpConnection connection, string? id, ReadOnlyMemory<byte> payload);