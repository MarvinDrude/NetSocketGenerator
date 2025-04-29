namespace NetSocketGenerator.Events;

/// <summary>
/// Represents a node in a trie structure used for organizing and matching patterns.
/// This node contains child TrieNode objects representing branching character paths
/// and associated pattern-handler pairs for handling specific patterns.
/// </summary>
/// <typeparam name="THandler">The type of handler associated with each pattern stored in the trie.</typeparam>
internal sealed class TrieNode<THandler>
{
   /// <summary>
   /// Gets the collection of child nodes associated with the current node,
   /// where each key in the dictionary represents a character and the value
   /// is a <see cref="TrieNode{THandler}"/> corresponding to the next branch in the trie structure.
   /// </summary>
   internal Dictionary<char, TrieNode<THandler>> Children { get; } = [];

   /// <summary>
   /// Gets the collection of pattern-handler pairs associated with the current trie node.
   /// Each entry in the list connects a specific <see cref="PatternInfo"/> to a corresponding handler of the specified type.
   /// </summary>
   internal List<PatternHandlerPair<THandler>> PatternHandlers { get; } = [];
}