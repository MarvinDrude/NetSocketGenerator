﻿namespace NetSocketGenerator.CacheQueue.Contracts.Messages.Queues;

public sealed class QueueSubscribeMessage : MessageBase
{
   public required string QueueName { get; set; }
}