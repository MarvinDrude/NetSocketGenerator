
namespace NetSocketGenerator.Tcp.Frames;

/// <inheritdoc cref="ITcpFrame"/>
public sealed class TcpFrame : ITcpFrame
{
   private string? _identifier;
   public string? Identifier
   {
      get => _identifier;
      set
      {
         if (value is { Length: > TcpConstants.SafeStackBufferSize })
         {
            throw new ArgumentException("Identifier cannot exceed the maximum buffer size", nameof(value));
         }
         
         _identifier = value;
      }
   }

   public ReadOnlyMemory<byte> Data { get; set; }
   
   [MemberNotNullWhen(false, nameof(Identifier))]
   public bool IsRawOnly { get; set; }
   public bool IsForSending { get; set; }
   public bool IsComplete { get; set; }
   
   public int GetRawSize()
   {
      if (IsRawOnly)
      {
         return 1 + Data.Length;
      }
      
      return 1 + Encoding.UTF8.GetByteCount(Identifier) + Data.Length;
   }
   
   public void Write(ref Span<byte> buffer)
   {
      buffer[0] = (byte)(IsRawOnly ? 1 : 0);
      buffer = buffer[1..];

      if (IsRawOnly)
      {
         Data.Span.CopyTo(buffer);
         return;
      }
      
      Span<byte> idSpan = Encoding.UTF8.GetBytes(Identifier); // TODO: could be faster maybe
      
      BinaryPrimitives.WriteInt32BigEndian(buffer[..4], idSpan.Length);
      buffer = buffer[4..];
      
      idSpan.CopyTo(buffer[..idSpan.Length]);
      buffer = buffer[idSpan.Length..];
      
      BinaryPrimitives.WriteInt32BigEndian(buffer[..4], Data.Length);
      buffer = buffer[4..];
      
      Data.Span.CopyTo(buffer);
   }
   
   public SequencePosition Read(ref ReadOnlySequence<byte> buffer)
   {
      var reader = new SequenceReader<byte>(buffer);
      
      if (!reader.TryRead(out var rawFlag))
      {
         return reader.Position;
      }
      
      IsRawOnly = rawFlag == 1;
      var sizeRequired = GetRawSize() - 1;

      if (reader.Remaining < sizeRequired)
      {
         return reader.Position;
      }

      if (IsRawOnly)
      {
         Memory<byte> memory = new byte[sizeRequired];
         Data = memory;
         
         reader.TryCopyTo(memory.Span);
         reader.Advance(memory.Span.Length);
         
         IsComplete = true;
         return reader.Position;
      }
      
      reader.TryReadBigEndian(out int idLength);
      
      if (idLength is > TcpConstants.SafeStackBufferSize or < 1)
         throw new InvalidOperationException();
      
      Span<byte> idSpan = stackalloc byte[idLength];

      reader.TryCopyTo(idSpan);
      reader.Advance(idSpan.Length);
      Identifier = Encoding.UTF8.GetString(idSpan);
      
      reader.TryReadBigEndian(out int dataLength);
      
      Memory<byte> payload = new byte[dataLength];
      Data = payload;
         
      reader.TryCopyTo(payload.Span);
      reader.Advance(payload.Span.Length);

      return reader.Position;
   }
   
   public void Dispose()
   {
      
   }
}