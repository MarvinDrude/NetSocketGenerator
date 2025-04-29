namespace NetSocketGenerator.Attributes;

/// <summary>
/// An attribute used to indicate that a method parameter represents the payload of a socket message.
/// This can also automatically use the configured serializer to give you the deserialized object.
/// </summary>
[AttributeUsage(AttributeTargets.Parameter, AllowMultiple = false, Inherited = false)]
public sealed class SocketPayloadAttribute : Attribute
{
   
}