
namespace NetSocketGenerator.Events;

/// <summary>
/// Represents an abstract base class for dispatching TCP frames to appropriate handlers
/// based on the frame's identifier or to raw handlers when the frame is marked as raw.
/// </summary>
/// <typeparam name="THandler">The type of handler to be used for processing frames.</typeparam>
public abstract class FrameDispatcher<THandler>
{
   /// <summary>
   /// Stores handlers categorized by their associated event identifiers.
   /// Each event identifier maps to a list of handlers that will be invoked
   /// when a frame with the corresponding identifier is dispatched.
   /// </summary>
   private readonly Dictionary<string, List<THandler>> _handlers = [];

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

      if (frame.Identifier is null 
          || !_handlers.TryGetValue(frame.Identifier, out var handlers))
      {
         return;
      }

      foreach (var keyHandler in handlers)
      {
         await DispatchKey(frame, connection, keyHandler);
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
      if (!_handlers.TryGetValue(eventName, out var handlers))
      {
         handlers = _handlers[eventName] = [];
      }
      handlers.Add(handler);
   }
}