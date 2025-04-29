namespace NetSocketGenerator.Events;

/// <summary>
/// Represents the information about a parsing pattern, including its tokens, length constraints,
/// and characteristics such as whether it starts with a literal.
/// </summary>
internal sealed class PatternInfo
{
   /// <summary>
   /// Gets the array of tokens that define the parsing pattern.
   /// </summary>
   /// <remarks>
   /// Tokens represent individual components or elements in the parsing logic of a pattern.
   /// They can include literals, wildcards, character ranges, or other specific matching rules.
   /// </remarks>
   public Token[] Tokens { get; }

   /// <summary>
   /// Gets the minimum length of the input required to match the pattern.
   /// </summary>
   /// <remarks>
   /// The <c>MinLength</c> property defines the smallest allowable input size that can successfully match the pattern.
   /// It is calculated based on the tokens in the pattern, incorporating elements such as literals and required character components.
   /// This value serves as a pre-check for input validation to avoid unnecessary matching attempts.
   /// </remarks>
   public int MinLength { get; private set; }

   /// <summary>
   /// Gets or sets the maximum allowable length for a pattern match.
   /// </summary>
   /// <remarks>
   /// This property defines the upper boundary for the length of the input that can match the pattern.
   /// If the input length exceeds this value, it is considered invalid for matching.
   /// A value of <c>null</c> indicates no upper limit, allowing the pattern to match input of any length within other constraints.
   /// </remarks>
   public int? MaxLength { get; private set; }

   /// <summary>
   /// Gets a value indicating whether the pattern begins with a literal token.
   /// </summary>
   /// <remarks>
   /// This property determines if the first token in the pattern is of type literal.
   /// A literal token represents a specific, fixed value that must match at the start
   /// of the input for the pattern to be considered valid.
   /// </remarks>
   public bool StartsWithLiteral { get; }

   /// <summary>
   /// Gets the first literal token in the pattern, if the pattern starts with a literal.
   /// </summary>
   /// <remarks>
   /// This property represents the initial literal text that the pattern mandates as a prefix, if applicable.
   /// If the pattern does not start with a literal token, this property will return null.
   /// </remarks>
   public string? FirstLiteral { get; }

   /// <summary>
   /// Encapsulates information about a pattern made up of tokens, including minimum and maximum length constraints
   /// and details about the starting literal if applicable. This class is responsible for evaluating patterns
   /// and managing associated tokens.
   /// </summary>
   public PatternInfo(Token[] tokens)
   {
      if (tokens.Length == 0)
         throw new ArgumentException("Pattern must contain at least one token", nameof(tokens));
      
      Tokens = tokens;
      ref var firstToken = ref tokens[0];
      if (firstToken.Type == TokenType.Literal)
      {
         StartsWithLiteral = true;
         FirstLiteral = firstToken.Literal;
      }
      
      CalculateLengths();
   }

   /// <summary>
   /// Determines whether a given span of characters matches the pattern represented by the current object's tokens.
   /// This method considers the minimum and maximum length constraints, as well as the requirement
   /// to match starting literals if applicable.
   /// </summary>
   /// <param name="input">The read-only span of characters to be evaluated against the pattern.</param>
   /// <returns>True if the input matches the pattern, otherwise false.</returns>
   public bool IsMatch(ReadOnlySpan<char> input)
   {
      if (input.Length < MinLength)
      {
         return false;
      }

      if (MaxLength is not null && input.Length > MaxLength)
      {
         return false;
      }

      if (StartsWithLiteral && !input.StartsWith(FirstLiteral!.AsSpan()))
      {
         return false;
      }
      
      return TokenMatcher.Match(input, Tokens);
   }

   /// <summary>
   /// Computes the minimum and maximum possible lengths of a pattern based on its tokens.
   /// Updates the <c>MinLength</c> and <c>MaxLength</c> properties of the <c>PatternInfo</c> object
   /// to reflect the calculated values, which are derived by analyzing the types of tokens
   /// contained in the pattern.
   /// This method ensures that future matching operations and validations have accurate length
   /// constraints for quick checks and validations.
   /// </summary>
   private void CalculateLengths()
   {
      MaxLength = 0;
      
      foreach (var token in Tokens)
      {
         switch (token.Type)
         {
            case TokenType.Literal:
               MinLength += token.Literal!.Length;
               MaxLength += token.Literal!.Length;
               break;
            case TokenType.AnyChar:
            case TokenType.CharRange:
               MinLength += 1;
               MaxLength += 1;
               break;
            case TokenType.Star:
               MaxLength = null;
               break;
            case TokenType.Invalid:
            default:
               break;
         }
      }
   }
}