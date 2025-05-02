namespace NetSocketGenerator.CacheQueue.Client;

public sealed class CacheQueueClientOptions
{
   public required string Address { get; set; }
   
   public required int Port { get; init; }
   
   public required IServiceProvider ServiceProvider { get; init; }
}