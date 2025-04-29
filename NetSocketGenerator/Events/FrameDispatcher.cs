
namespace NetSocketGenerator.Events;

/// <summary>
/// Represents an abstract base class for dispatching TCP frames to appropriate handlers
/// based on the frame's identifier or to raw handlers when the frame is marked as raw.
/// </summary>
/// <typeparam name="THandler">The type of handler to be used for processing frames.</typeparam>
public abstract class FrameDispatcher<THandler>
{
   /// <summary>
   /// Represents the root node of a trie structure used for organizing and matching patterns
   /// against registered event handlers. This serves as the starting point for traversing
   /// and finding the appropriate handler for a given event identifier in the dispatching process.
   /// </summary>
   private readonly TrieNode<THandler> _treeRoot = new();

   /// <summary>
   /// Stores a collection of pattern handler entries where the patterns are matched
   /// against the beginning of event names with wildcard support. This is utilized
   /// to route events whose identifiers may partially match multiple handlers,
   /// especially when flexible or dynamic routing is required.
   /// </summary>
   private readonly Dictionary<string, PatternHandlerEntry<THandler>> _startWildcardHandlers = [];

   /// <summary>
   /// Maintains a list of handlers designated to process raw frames.
   /// These handlers are invoked when a frame is marked as raw-only,
   /// bypassing the categorization by event identifier.
   /// </summary>
   private readonly List<THandler> _rawHandlers = [];

   /// <summary>
   /// Dispatches a raw TCP frame to a specific handler when the frame is marked as raw.
   /// Attempts to process the frame using the provided raw handler.
   /// </summary>
   /// <param name="frame">The incoming raw TCP frame containing data to be processed.</param>
   /// <param name="connection">The TCP connection associated with the raw frame, or null if no specific connection is involved.</param>
   /// <param name="handler">The handler responsible for processing the raw frame.</param>
   /// <returns>A task that represents the asynchronous dispatching operation using the provided raw handler.</returns>
   protected abstract Task DispatchRaw(ITcpFrame frame, ITcpConnection? connection, THandler handler);

   /// <summary>
   /// Dispatches a TCP frame to a specific handler based on the frame's identifier.
   /// Attempts to process the frame using a key handler if available.
   /// </summary>
   /// <param name="frame">The incoming TCP frame containing data and metadata for processing.</param>
   /// <param name="connection">The TCP connection associated with the frame, or null if no specific connection is involved.</param>
   /// <param name="handler">The handler to process the frame based on its identifier.</param>
   /// <returns>A task that represents the asynchronous dispatching operation using the provided key handler.</returns>
   protected abstract Task DispatchKey(ITcpFrame frame, ITcpConnection? connection, THandler handler);

   /// <summary>
   /// Dispatches a TCP frame to appropriate handlers based on the frame's properties.
   /// Frames marked as raw are dispatched to raw handlers; otherwise, they are
   /// dispatched based on their identifier.
   /// </summary>
   /// <param name="frame">The TCP frame to dispatch, which contains the relevant data and metadata for processing.</param>
   /// <param name="connection">The TCP connection associated with the frame, or null if no specific connection is involved.</param>
   /// <returns>A task that represents the asynchronous dispatching operation.</returns>
   public async Task Dispatch(ITcpFrame frame, ITcpConnection? connection)
   {
      if (frame.IsRawOnly)
      {
         foreach (var handler in _rawHandlers)
         {
            await DispatchRaw(frame, connection, handler);
         }

         return;
      }

      var node = _treeRoot;
      ReadOnlySpan<char> identifier = frame.Identifier;
      List<Task> tasks = [];

      for (var index = 0; index < identifier.Length; index++)
      {
         if (!node.Children.TryGetValue(identifier[index], out node))
         {
            break;
         }

         foreach (var (pattern, handler) in node.PatternHandlers)
         {
            if (!pattern.IsMatch(identifier))
            {
               continue;
            }
            
            tasks.Add(DispatchKey(frame, connection, handler));
         }
      }
      
      await Task.WhenAll(tasks);
      
      foreach (var wildcardEntry in _startWildcardHandlers.Values)
      {
         if (!wildcardEntry.Pattern.IsMatch(frame.Identifier))
         {
            continue;
         }
         
         foreach (var handler in wildcardEntry.Handlers)
         {
            await DispatchKey(frame, connection, handler);
         }
      }
   }

   /// <summary>
   /// Adds a handler to the list of raw frame handlers. Raw handlers are designated
   /// for processing frames that are marked as raw-only, bypassing categorization
   /// by other identifiers.
   /// </summary>
   /// <param name="handler">The handler to add to the raw handlers list. The handler must implement the appropriate logic to process raw frames.</param>
   public void AddRawHandler(THandler handler)
   {
      _rawHandlers.Add(handler);
   }

   /// <summary>
   /// Adds a handler associated with a specific event name for processing TCP frames.
   /// </summary>
   /// <param name="eventName">The name of the event associated with the handler.</param>
   /// <param name="handler">The handler to add, which will process frames associated with the specified event name.</param>
   public void AddKeyHandler(string eventName, THandler handler)
   {
      var patternInfo = new PatternInfo(Tokenizer.Tokenize(eventName));

      if (patternInfo.StartsWithLiteral)
      {
         var literal = patternInfo.FirstLiteral!;
         var node = _treeRoot;

         foreach (var character in literal)
         {
            if (!node.Children.TryGetValue(character, out var next))
            {
               next = node.Children[character] = new TrieNode<THandler>();
            }

            node = next;
         }
         
         node.PatternHandlers.Add(new PatternHandlerPair<THandler>(patternInfo, handler));
         return;
      }

      if (!_startWildcardHandlers.TryGetValue(eventName, out var entry))
      {
         _startWildcardHandlers[eventName] = entry = new PatternHandlerEntry<THandler>(patternInfo);
      }
      
      entry.Handlers.Add(handler);
   }
}