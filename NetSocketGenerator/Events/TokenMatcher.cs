namespace NetSocketGenerator.Events;

/// <summary>
/// Provides functionality to check if a given span of characters matches a sequence of predefined tokens.
/// </summary>
internal static class TokenMatcher
{
   /// <summary>
   /// Determines if the provided input matches the specified sequence of tokens.
   /// </summary>
   /// <param name="input">The read-only span of characters representing the input to be evaluated.</param>
   /// <param name="tokens">An array of tokens against which the input is matched.</param>
   /// <returns>True if the input matches the token sequence, otherwise false.</returns>
   public static bool Match(ReadOnlySpan<char> input, Token[] tokens)
   {
      return IsMatchRecursive(input, 0, tokens, 0);
   }

   /// <summary>
   /// Recursively determines if the provided input matches the specified sequence of tokens starting from the given positions.
   /// </summary>
   /// <param name="input">The read-only span of characters representing the input to be evaluated.</param>
   /// <param name="position">The current position in the input span that is being evaluated.</param>
   /// <param name="tokens">An array of tokens against which the input is being recursively matched.</param>
   /// <param name="indexToken">The current index in the token array that is being matched.</param>
   /// <returns>True if the input matches the token sequence at the specified positions, otherwise false.</returns>
   private static bool IsMatchRecursive(
      ReadOnlySpan<char> input, int position, Token[] tokens, int indexToken)
   {
      if (indexToken == tokens.Length)
      {
         return position == input.Length;
      }
      
      ref var token = ref tokens[indexToken];

      if (token.Type == TokenType.Star)
      {
         // stars, lets just try every possible skip until input is exhausted
         for (var skip = 0; skip < input.Length - position; skip++)
         {
            if (IsMatchRecursive(input, position + skip, tokens, indexToken + 1))
            {
               return true;
            }
         }

         return false;
      }

      if (position >= input.Length)
      {
         return false;
      }

      return token.Type switch
      {
         TokenType.Literal => IsMatchLiteral(in token, input, position, tokens, indexToken),
         TokenType.AnyChar => IsMatchRecursive(input, position + 1, tokens, indexToken + 1),
         TokenType.CharRange => IsMatchCharRange(in token, input, position, tokens, indexToken),
         _ => false,
      };
   }

   /// <summary>
   /// Determines if a literal token matches the specified portion of the input and recursively evaluates the next tokens if a match is found.
   /// </summary>
   /// <param name="token">The literal token to be matched against the input.</param>
   /// <param name="input">The read-only span of characters representing the input to be evaluated.</param>
   /// <param name="position">The current position within the input span where the match should begin.</param>
   /// <param name="tokens">An array of tokens representing the sequence to be matched.</param>
   /// <param name="indexToken">The current index of the token being evaluated within the token array.</param>
   /// <returns>True if the literal token matches the input at the specified position and subsequent tokens continue to match, otherwise false.</returns>
   private static bool IsMatchLiteral(
      in Token token, ReadOnlySpan<char> input, int position, Token[] tokens, int indexToken)
   {
      var literal = token.Literal!.AsSpan();

      if (position + literal.Length <= input.Length
          && input.Slice(position, literal.Length).SequenceEqual(literal))
      {
         return IsMatchRecursive(input, position + literal.Length, tokens, indexToken + 1);
      }
      
      return false;
   }

   /// <summary>
   /// Determines if the character at the specified position in the input matches a specified character range token
   /// and validates the remaining input recursively against the remaining tokens.
   /// </summary>
   /// <param name="token">The token representing the character range to match.</param>
   /// <param name="input">The read-only span of characters representing the input being evaluated.</param>
   /// <param name="position">The current position in the input being checked.</param>
   /// <param name="tokens">The array of tokens against which the input is being matched.</param>
   /// <param name="indexToken">The current index of the token being evaluated within the token array.</param>
   /// <returns>True if the character in the input matches the character range token and the remainder of the input matches remaining tokens, otherwise false.</returns>
   private static bool IsMatchCharRange(
      in Token token, ReadOnlySpan<char> input, int position, Token[] tokens, int indexToken)
   {
      return token.Literal!.Contains(input[position])
             && IsMatchRecursive(input, position + 1, tokens, indexToken + 1);
   }
}