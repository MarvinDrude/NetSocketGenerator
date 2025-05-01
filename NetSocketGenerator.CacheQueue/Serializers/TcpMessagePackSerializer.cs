using MessagePack;

namespace NetSocketGenerator.CacheQueue.Serializers;

public sealed class TcpMessagePackSerializer : ITcpSerializer
{
   public T? Deserialize<T>(ReadOnlySpan<byte> source, ReadOnlyMemory<byte> sourceMemory)
   {
      return MessagePackSerializer.Deserialize<T>(sourceMemory);
   }

   public ReadOnlyMemory<byte> SerializeAsMemory<T>(T target)
   {
      return MessagePackSerializer.Serialize(target);
   }

   public ReadOnlySpan<byte> SerializeAsSpan<T>(T target)
   {
      return MessagePackSerializer.Serialize(target);
   }
}