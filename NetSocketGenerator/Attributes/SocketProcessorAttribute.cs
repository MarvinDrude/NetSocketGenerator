namespace NetSocketGenerator.Attributes;

/// <summary>
/// Specifies that the attributed class is a socket processor.
/// </summary>
/// <remarks>
/// This attribute is used to mark a class as a socket processor,
/// and it requires an event name pattern to identify or match specific events.
/// </remarks>
/// <example>
/// Classes marked with <c>SocketProcessorAttribute</c> can define custom logic to process socket events.
/// </example>
[AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
public sealed class SocketProcessorAttribute : Attribute
{
   /// <summary>
   /// Gets or sets the pattern used to identify or match event names.
   /// </summary>
   /// <remarks>
   /// This property defines a required pattern that is utilized for recognizing specific event names.
   /// Use this property to specify a consistent naming scheme for events that the attributed class is intended to process.
   /// </remarks>
   public required string EventNamePattern { get; init; }

   /// <summary>
   /// Optionally set Include Server to false, if you don't want this handler to be on the server
   /// exention method ServiceCollection.AddSocketServerProcessors() and TcpServer.UseSocketServerProcessors()
   /// </summary>
   public bool IncludeServer { get; init; } = true;
   
   /// <summary>
   /// Optionally set Include Client to false, if you don't want this handler to be on the client
   /// extension method ServiceCollection.AddSocketClientProcessors() and TcpClient.UseSocketClientProcessors()
   /// </summary>
   public bool IncludeClient { get; init; } = true;
}