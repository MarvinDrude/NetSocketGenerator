namespace NetSocketGenerator.Extensions;

/// <summary>
/// Provides extension methods for working with Memory and ReadOnlyMemory buffer types to retrieve underlying array segments.
/// </summary>
public static class BufferExtensions
{
   /// <summary>
   /// Retrieves the underlying array segment from a writable <see cref="Memory{T}"/> buffer.
   /// </summary>
   /// <param name="memory">The <see cref="Memory{T}"/> buffer to retrieve the array segment from.</param>
   /// <returns>An <see cref="ArraySegment{T}"/> representing the underlying array of the buffer.</returns>
   /// <exception cref="InvalidOperationException">Thrown if the buffer is not backed by an array.</exception>
   public static ArraySegment<byte> GetArray(this Memory<byte> memory)
   {
      return ((ReadOnlyMemory<byte>)memory).GetArray();
   }

   /// <summary>
   /// Retrieves the underlying array segment from a writable <see cref="Memory{T}"/> buffer.
   /// </summary>
   /// <param name="memory">The <see cref="Memory{T}"/> buffer to retrieve the array segment from.</param>
   /// <returns>An <see cref="ArraySegment{T}"/> representing the underlying array of the buffer.</returns>
   /// <exception cref="InvalidOperationException">Thrown if the buffer is not backed by an array.</exception>
   public static ArraySegment<byte> GetArray(this ReadOnlyMemory<byte> memory)
   {
      if (!MemoryMarshal.TryGetArray(memory, out var result))
      {
         throw new InvalidOperationException("Buffer backed by array was expected");
      }

      return result;
   }
}