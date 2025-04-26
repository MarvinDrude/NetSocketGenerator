
namespace NetSocketGenerator.Tcp.Frames;

/// <inheritdoc cref="ITcpFrame"/>
public sealed class TcpFrame : ITcpFrame
{
   public string? Identifier { get; set; }
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
      
   }
   
   private SequencePosition
   
   public void Dispose()
   {
      // TODO release managed resources here
   }
}