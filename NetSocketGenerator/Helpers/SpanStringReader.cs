namespace NetSocketGenerator.Helpers;

/// <summary>
/// Represents a utility for efficiently reading through a span of characters in a sequential manner.
/// </summary>
internal ref struct SpanStringReader
{
   /// <summary>
   /// Represents the span of characters being read by the reader.
   /// This field serves as the source span that the reader operates on,
   /// providing a memory-efficient way to process and traverse the characters
   /// without additional allocations.
   /// </summary>
   private readonly ReadOnlySpan<char> _span;

   /// <summary>
   /// Tracks the current position within the span being read.
   /// This field is updated as the reader progresses through the span,
   /// enabling sequential reading and facilitating operations
   /// like advancing, resetting, and determining whether the end
   /// of the span has been reached.
   /// </summary>
   private int _position;

   /// <summary>
   /// Provides a utility for reading through a span of characters in a sequential and memory-efficient manner.
   /// This structure allows inspecting, reading, and processing data from a ReadOnlySpan by tracking
   /// a current position dynamically within the span.
   /// </summary>
   public SpanStringReader(ReadOnlySpan<char> span)
   {
      _span = span;
      _position = 0;
   }

   /// <summary>
   /// Indicates whether the reader has reached the end of the span.
   /// This property returns true if the current reader position is at or beyond
   /// the end of the underlying span; otherwise, it returns false.
   /// </summary>
   public bool IsAtEnd => _position >= _span.Length;

   /// <summary>
   /// Gets the character at the current reader position in the span.
   /// This property does not advance the reader's position.
   /// </summary>
   public char Current => _span[_position];

   /// <summary>
   /// Gets the remaining portion of the span from the current position to the end.
   /// This property provides a view of the unread part of the span without modifying the reader's state.
   /// </summary>
   public ReadOnlySpan<char> Remaining => _span[_position..];

   /// <summary>
   /// Reads the current character at the reader's position and advances the position to the next character.
   /// If the reader is at the end of the span, '\0' is returned to indicate no characters are left to be read.
   /// </summary>
   /// <returns>The current character, or '\0' if the reader has reached the end of the span.</returns>
   public char Read()
   {
      return IsAtEnd
         ? '\0' : _span[_position++];
   }

   /// <summary>
   /// Reads characters from the current position in the span while the specified predicate evaluates to true.
   /// The method moves the reader's position to the first character that does not satisfy the predicate or
   /// to the end of the span.
   /// </summary>
   /// <param name="predicate">A function to evaluate each character. Reading continues while this function returns true.</param>
   /// <returns>A slice of the span containing the characters that satisfy the predicate.</returns>
   public ReadOnlySpan<char> ReadWhile(Func<char, bool> predicate)
   {
      var start = _position;
      while (_position < _span.Length && predicate(_span[_position]))
      {
         _position++;
      }
      return _span.Slice(start, _position - start);
   }

   /// <summary>
   /// Advances the reader's current position by a specified number of characters.
   /// If the specified count exceeds the remaining characters, the position is
   /// moved to the end of the span.
   /// </summary>
   /// <param name="count">The number of characters to advance. Defaults to 1.</param>
   public void Advance(int count = 1)
   {
      _position = Math.Min(_position + count, _span.Length);
   }

   /// <summary>
   /// Resets the reader's position to the beginning of the span.
   /// This method is useful for restarting the reading process
   /// from the start without reinitializing the SpanStringReader.
   /// </summary>
   public void Reset()
   {
      _position = 0;
   }
}