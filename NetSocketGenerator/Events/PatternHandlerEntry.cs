namespace NetSocketGenerator.Events;

/// <summary>
/// Represents an entry that associates a pattern with a collection of handlers.
/// This class is used to map a specific <see cref="PatternInfo"/> to its associated
/// handlers of the specified type.
/// </summary>
/// <typeparam name="THandler">The type of the handler associated with the pattern.</typeparam>
internal sealed class PatternHandlerEntry<THandler>(PatternInfo pattern)
{
   /// <summary>
   /// Gets the collection of handlers associated with the specific pattern.
   /// </summary>
   /// <remarks>
   /// This property provides access to the list of handlers that are mapped to a pattern within the <see cref="PatternHandlerEntry{THandler}"/> class.
   /// The handlers are executed when a corresponding pattern is matched.
   /// </remarks>
   public List<THandler> Handlers { get; } = [];

   /// <summary>
   /// Gets the pattern information associated with the entry.
   /// </summary>
   /// <remarks>
   /// This property provides access to the <see cref="PatternInfo"/> instance associated with the current entry
   /// in the <see cref="PatternHandlerEntry{THandler}"/> class. The pattern contains details about the structure,
   /// tokens, and evaluation characteristics necessary for handling input matching operations.
   /// </remarks>
   public PatternInfo Pattern { get; } = pattern;
}

/// <summary>
/// Represents a pair that associates a pattern with a single handler.
/// This class is used to map a specific <see cref="PatternInfo"/> to an individual
/// handler of the specified type.
/// </summary>
/// <typeparam name="THandler">The type of the handler associated with the pattern.</typeparam>
internal sealed class PatternHandlerPair<THandler>(
   PatternInfo pattern,
   THandler handler)
{
   /// <summary>
   /// Gets the pattern information associated with this handler entry.
   /// </summary>
   /// <remarks>
   /// This property provides access to the detailed information about the pattern used in this entry.
   /// The pattern defines the structure or format recognized by the handler, encapsulated in a <see cref="PatternInfo"/> object.
   /// </remarks>
   public PatternInfo Pattern { get; } = pattern;

   /// <summary>
   /// Gets the handler associated with the specific pattern.
   /// </summary>
   /// <remarks>
   /// This property provides access to the handler that is paired with a pattern
   /// in the <see cref="PatternHandlerPair{THandler}"/> class. The handler is
   /// executed to process the pattern when a match is found.
   /// </remarks>
   public THandler Handler { get; } = handler;

   /// <summary>
   /// Deconstructs the <see cref="PatternHandlerPair{THandler}"/> into its constituent components:
   /// the associated <see cref="PatternInfo"/> and the handler.
   /// </summary>
   /// <param name="pattern">Outputs the <see cref="PatternInfo"/> associated with this pair.</param>
   /// <param name="handler">Outputs the handler of type <typeparamref name="THandler"/> associated with this pair.</param>
   public void Deconstruct(out PatternInfo pattern, out THandler handler)
   {
      pattern = Pattern;
      handler = Handler;
   }
}