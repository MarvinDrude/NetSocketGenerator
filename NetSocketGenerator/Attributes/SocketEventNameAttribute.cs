namespace NetSocketGenerator.Attributes;

/// <summary>
/// Specifies that a parameter within a method represents the event name associated with a socket operation.
/// </summary>
/// <remarks>
/// This attribute is used to annotate parameters that are intended to hold the name of socket events in a system
/// where events are identified by specific names. It ensures clarity of purpose and facilitates tooling or
/// code generation processes that rely on such metadata.
/// </remarks>
[AttributeUsage(AttributeTargets.Parameter, AllowMultiple = false, Inherited = false)]
public sealed class SocketEventNameAttribute : Attribute
{
   
}