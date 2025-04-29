namespace NetSocketGenerator.Events;

internal sealed class Tokenizer
{
   /// <summary>
   /// Tokenizes the input span of characters into an array of tokens, processing each character or sequence of characters based on specific rules.
   /// </summary>
   /// <param name="input">The input character span to tokenize.</param>
   /// <returns>An array of tokens parsed from the input.</returns>
   public static Token[] Tokenize(ReadOnlySpan<char> input)
   {
      List<Token> tokens = [];
      var reader = new SpanStringReader(input);

      while (!reader.IsAtEnd)
      {
         var current = reader.Current;

         if (IsNormalCharSequence(current))
         {
            var run = reader.ReadWhile(IsNormalCharSequence);
            tokens.Add(Token.CreateLiteral(run.ToString()));

            continue;
         }

         var token = reader.Read() switch
         {
            '?' => Token.CreateAnyChar(),
            '*' => Token.CreateStar(),
            '\\' when !reader.IsAtEnd => Token.CreateLiteral(reader.Read().ToString()),
            '[' => GetCharRangeToken(ref reader),
            _ => Token.CreateInvalid()
         };

         if (token.Type != TokenType.Invalid)
         {
            tokens.Add(token);
         }
      }
      
      return [.. tokens];
   }

   /// <summary>
   /// Parses a character range from the provided reader and constructs a character range token.
   /// </summary>
   /// <param name="reader">The <c>SpanStringReader</c> to read characters from.</param>
   /// <returns>A token representing the character range if a valid range is detected.</returns>
   /// <exception cref="InvalidOperationException">Thrown when an unclosed character range is encountered.</exception>
   private static Token GetCharRangeToken(ref SpanStringReader reader)
   {
      var isClosed = false;
      Span<char> run = stackalloc char[TcpConstants.TokenCharRangeMaxCount];
      var currentRunIndex = 0;
      
      while (!reader.IsAtEnd)
      {
         var current = reader.Read();
         
         if (current == '\\' && !reader.IsAtEnd)
         {
            current = reader.Read();
            run[currentRunIndex++] = current;
         }
         else if (current == ']')
         {
            isClosed = true;
            break;
         }
         else
         {
            run[currentRunIndex++] = current;
         }
      }

      if (isClosed)
      {
         return Token.CreateCharRange(run[..currentRunIndex]);
      }

      throw new InvalidOperationException("Unclosed character range started with [ but no closing ] was found.");
   }

   /// <summary>
   /// Determines if the specified character represents a normal character sequence that can be part of a valid token.
   /// </summary>
   /// <param name="c">The character to evaluate.</param>
   /// <returns>True if the character is part of a normal sequence; otherwise, false.</returns>
   private static bool IsNormalCharSequence(char c) => c is not '?' and not '*' and not '[' and not '\\';
}

internal readonly struct Token(
   TokenType type,
   string? literal)
{
   public readonly TokenType Type = type;
   public readonly string? Literal = literal;
   
   public static Token CreateLiteral(string literal) => new(TokenType.Literal, literal);
   public static Token CreateAnyChar() => new(TokenType.AnyChar, null);
   public static Token CreateStar() => new(TokenType.Star, null);
   public static Token CreateCharRange(Span<char> chars) => new(TokenType.CharRange, chars.ToString());
   public static Token CreateInvalid() => new(TokenType.Invalid, null);
}

/// <summary>
/// Represents the type of tokens that can be emitted during tokenization of input strings.
/// </summary>
internal enum TokenType
{
   /// <summary>
   /// Represents an invalid token type detected during the tokenization process.
   /// </summary>
   /// <remarks>
   /// This enumeration member is used to indicate that an unrecognized or unsupported token
   /// was encountered in the input string. It serves as a marker for invalid syntax or
   /// unexpected characters during parsing.
   /// </remarks>
   Invalid = 0,

   /// <summary>
   /// Represents a token type that holds a sequence of literal characters from the input.
   /// </summary>
   /// <remarks>
   /// This enumeration member is used to identify tokens that consist of one or more regular
   /// characters that are treated as literals. These characters do not have any special meaning
   /// in the tokenization context and are directly captured as part of the token output.
   /// </remarks>
   Literal = 1,

   /// <summary>
   /// Represents a token that matches any single character.
   /// </summary>
   /// <remarks>
   /// This enumeration member is used when the input pattern includes a wildcard character
   /// that can match any single character. It is typically associated with the '?' symbol
   /// during tokenization of input strings.
   /// </remarks>
   AnyChar = 2,

   /// <summary>
   /// Represents a token type used to match zero or more characters in the input string.
   /// </summary>
   /// <remarks>
   /// This enumeration member is utilized in tokenization to identify patterns
   /// where an asterisk (*) symbol is encountered. It signifies that the token can match
   /// any sequence of characters, including an empty sequence, during parsing or pattern evaluation.
   /// </remarks>
   Star = 3,

   /// <summary>
   /// Represents a token type corresponding to a character range definition.
   /// </summary>
   /// <remarks>
   /// This enumeration member is used during the tokenization process to identify a token
   /// that defines a range of characters, typically specified within brackets (e.g., [a-z]).
   /// It is utilized for parsing patterns involving character groups or ranges.
   /// </remarks>
   CharRange = 4,
}