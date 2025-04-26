namespace NetSocketGenerator.Events.Delegates;

public delegate Task FrameMessageHandler(string? id, ReadOnlyMemory<byte> payload);