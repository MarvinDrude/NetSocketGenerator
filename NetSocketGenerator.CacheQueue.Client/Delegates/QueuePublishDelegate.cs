namespace NetSocketGenerator.CacheQueue.Client.Delegates;

public delegate Task QueuePublishDelegate<T>(QueuePublishContext<T> context);