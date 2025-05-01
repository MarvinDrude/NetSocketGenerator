
namespace NetSocketGenerator.Tcp.Interfaces;

public interface ITcpSerializer
{
   public T? Deserialize<T>(ReadOnlySpan<byte> source, ReadOnlyMemory<byte> sourceMemory);

   public ReadOnlyMemory<byte> SerializeAsMemory<T>(T target);

   public ReadOnlySpan<byte> SerializeAsSpan<T>(T target);
}