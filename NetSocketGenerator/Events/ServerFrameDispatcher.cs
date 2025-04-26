namespace NetSocketGenerator.Events;

/// <summary>
/// A sealed implementation of FrameDispatcher for server-side frame handling.
/// ServerFrameDispatcher routes TCP frames to the associated ServerFrameMessageHandler
/// for further processing.
/// </summary>
public sealed class ServerFrameDispatcher 
   : FrameDispatcher<ServerFrameMessageHandler>
{
   /// <summary>
   /// Dispatches the raw TCP frame to the specified message handler for processing.
   /// </summary>
   /// <param name="frame">The TCP frame containing the identifier and data to be processed.</param>
   /// <param name="connection">The TCP connection associated with the frame.</param>
   /// <param name="handler">The delegate handler that processes the frame data.</param>
   /// <returns>A task representing the asynchronous operation of dispatching the frame.</returns>
   protected override Task DispatchRaw(
      ITcpFrame frame, ITcpConnection? connection, ServerFrameMessageHandler handler)
   {
      return handler(connection!, frame.Identifier, frame.Data);
   }

   /// <summary>
   /// Dispatches a specific key from the TCP frame to the provided server frame message handler for processing.
   /// </summary>
   /// <param name="frame">The TCP frame containing the identifier and payload to be processed.</param>
   /// <param name="connection">The TCP connection associated with the frame. Can be null if not applicable.</param>
   /// <param name="handler">The server frame message handler delegate responsible for processing the key and payload.</param>
   /// <returns>A task representing the asynchronous operation of dispatching the key and payload.</returns>
   protected override Task DispatchKey(
      ITcpFrame frame, ITcpConnection? connection, ServerFrameMessageHandler handler)
   {
      return handler(connection!, frame.Identifier, frame.Data);
   }
}