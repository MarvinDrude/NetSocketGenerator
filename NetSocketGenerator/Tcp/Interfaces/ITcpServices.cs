
namespace NetSocketGenerator.Tcp.Interfaces;

public interface ITcpServices
{
   public IServiceProvider Services { get; }

   public IServiceScope CreateScope();
}