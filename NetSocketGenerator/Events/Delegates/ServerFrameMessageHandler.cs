namespace NetSocketGenerator.Events.Delegates;

public delegate Task ServerFrameMessageHandler(string? id, ReadOnlyMemory<byte> payload);