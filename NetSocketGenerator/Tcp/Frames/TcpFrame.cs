
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
         return 1 + 4 + Data.Length;
      }
      
      return 1 + 4 + 4 + Encoding.UTF8.GetByteCount(Identifier) + Data.Length;
   }
   
   public void Write(ref Span<byte> input)
   {
      var buffer = input;
      
      buffer[0] = (byte)(IsRawOnly ? 1 : 0);
      buffer = buffer[1..];

      Span<byte> idSpan = Encoding.UTF8.GetBytes(Identifier ?? string.Empty); // TODO: could be faster maybe
      
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
      
      if (_parseStep == ParseStep.BeforeRaw)
      {
         if (reader.Remaining < 1)
         {
            return reader.Position;
         }
         
         reader.TryRead(out var isRawOnly);
         IsRawOnly = isRawOnly == 1;
         
         _parseStep = ParseStep.AfterRaw;
      }

      if (_parseStep == ParseStep.AfterRaw)
      {
         if (reader.Remaining < 4)
         {
            return reader.Position;
         }

         reader.TryReadBigEndian(out _idLength);
         _parseStep = ParseStep.Id;
      }

      if (_parseStep == ParseStep.Id)
      {
         if (reader.Remaining < _idLength)
         {
            return reader.Position;
         }

         if (_idLength > 0)
         {
            if (_idLength > TcpConstants.SafeStackBufferSize)
               throw new InvalidOperationException();
         
            Span<byte> idSpan = stackalloc byte[_idLength];

            reader.TryCopyTo(idSpan);
            reader.Advance(_idLength);
         
            Identifier = Encoding.UTF8.GetString(idSpan);
         }
         
         _parseStep = ParseStep.DataLength;
      }

      if (_parseStep == ParseStep.DataLength)
      {
         if (reader.Remaining < 4)
         {
            return reader.Position;
         }
         
         reader.TryReadBigEndian(out _dataLength);
         _parseStep = ParseStep.Data;
      }

      if (_parseStep == ParseStep.Data)
      {
         if (reader.Remaining < _dataLength)
         {
            return reader.Position;
         }
         
         Memory<byte> payload = new byte[_dataLength];
         Data = payload;
         
         reader.TryCopyTo(payload.Span);
         reader.Advance(payload.Span.Length);
         
         _parseStep = ParseStep.Finish;
         IsComplete = true;
      }

      return reader.Position;
   }
   
   public void Dispose()
   {
      
   }

   private ParseStep _parseStep = ParseStep.BeforeRaw;
   private int _idLength;
   private int _dataLength;
   
   private enum ParseStep
   {
      BeforeRaw = 1,
      AfterRaw = 2,
      Id = 3,
      DataLength = 4,
      Data = 5,
      Finish = 6
   }
}