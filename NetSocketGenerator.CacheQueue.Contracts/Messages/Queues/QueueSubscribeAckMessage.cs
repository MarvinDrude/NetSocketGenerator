﻿namespace NetSocketGenerator.CacheQueue.Contracts.Messages.Queues;

public sealed class QueueSubscribeAckMessage : AckMessageBase
{
   public required bool IsFound { get; set; }
   
   public required bool IsSubscribed { get; set; }
}