using System.Text.Encodings.Web;
using System.Text.Json;

namespace NetSocketGenerator.Tcp.Serializers;

public sealed class TcpDynamicJsonSerializer : ITcpSerializer
{
   private const int InitialBufferSize = 1024 * 2;
   
   private static readonly JsonSerializerOptions JsonOptions = new() {
      IncludeFields = true,
      PropertyNameCaseInsensitive = true,
      Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
      PropertyNamingPolicy = JsonNamingPolicy.CamelCase
   };

   private static readonly JsonWriterOptions WriterOptions = new() {
      Indented = false,
      Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
   };

   public T? Deserialize<T>(ReadOnlySpan<byte> source, ReadOnlyMemory<byte> sourceMemory)
   {
      return JsonSerializer.Deserialize<T>(source, JsonOptions);
   }

   public ReadOnlyMemory<byte> SerializeAsMemory<T>(T target)
   {
      ArrayBufferWriter<byte> buffer = new(InitialBufferSize);
      FillWriterBuffer(buffer, target);

      return buffer.WrittenMemory;
   }

   public ReadOnlySpan<byte> SerializeAsSpan<T>(T target)
   {
      ArrayBufferWriter<byte> buffer = new(InitialBufferSize);
      FillWriterBuffer(buffer, target);

      return buffer.WrittenSpan;
   }
   
   private static void FillWriterBuffer<T>(ArrayBufferWriter<byte> buffer, T target) 
   {
      using Utf8JsonWriter writer = new(buffer, WriterOptions);
      JsonSerializer.Serialize(writer, target, JsonOptions);
   }
}