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
   /// exention method ServiceCollection.AddSocketServerProcessors() and TcpServer.UseServerProcessors()
   /// </summary>
   public bool IncludeServer { get; init; } = true;
   
   /// <summary>
   /// Optionally set Include Client to false, if you don't want this handler to be on the client
   /// extension method ServiceCollection.AddSocketClientProcessors() and TcpClient.UseSocketProcessors()
   /// </summary>
   public bool IncludeClient { get; init; } = true;

   /// <summary>
   /// Gets or sets the registration groups for the socket processor.
   /// </summary>
   /// <remarks>
   /// This property defines a set of groups that the socket processor is associated with.
   /// Will put in the general extension methods, but als in ServiceCollection.AddSocket{Type}{GroupName}Processors();
   /// and TcpServer/TcpClient.UseSocket{GroupName}Processors();
   /// </remarks>
   public string[] RegistrationGroups { get; init; } = [];
}