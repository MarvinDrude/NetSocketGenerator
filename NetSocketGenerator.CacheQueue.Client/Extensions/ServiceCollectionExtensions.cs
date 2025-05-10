namespace NetSocketGenerator.CacheQueue.Client.Extensions;

public static class ServiceCollectionExtensions
{
   public static IServiceCollection AddCacheQueueClient(this IServiceCollection services)
   {
      return services
         .AddSocketClientQueueProcessors()
         .AddSocketClientCommandsProcessors();
   }
}