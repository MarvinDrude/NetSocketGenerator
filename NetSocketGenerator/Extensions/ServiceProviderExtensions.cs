namespace NetSocketGenerator.Extensions;

public static class ServiceProviderExtensions
{
   public static T GetRequiredServiceObject<T>(this IServiceProvider provider) 
      where T : notnull
   {
      return provider.GetRequiredService<T>();
   }
}