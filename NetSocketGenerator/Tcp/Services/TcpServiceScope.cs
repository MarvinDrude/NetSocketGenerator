namespace NetSocketGenerator.Tcp.Services;

public sealed class TcpServiceScope(IServiceScope scope) : IDisposable
{
   public IServiceProvider ServiceProvider => scope.ServiceProvider;
   
   public void Dispose()
   {
      scope.Dispose();
   }
}