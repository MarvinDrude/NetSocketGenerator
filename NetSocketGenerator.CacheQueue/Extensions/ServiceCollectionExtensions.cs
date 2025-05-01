
namespace NetSocketGenerator.CacheQueue.Extensions;

public static class ServiceCollectionExtensions
{
   public static IServiceCollection AddCacheQueue(this IServiceCollection services)
   {
      return services
         .AddSocketServerQueueProcessors();
   }
}