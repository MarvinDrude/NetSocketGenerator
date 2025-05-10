using System.Text.Json.Serialization;

namespace NetSocketGenerator.CacheQueue.Contracts.Messages;

[JsonPolymorphic]
[JsonDerivedType(typeof(AckMessageBase), "base")]
[JsonDerivedType(typeof(SetStringCommandAck), CommandNames.StringSet)]
[JsonDerivedType(typeof(GetStringCommandAck), CommandNames.StringGet)]
public class AckMessageBase
{
   public Guid RequestId { get; } = Guid.CreateVersion7();
   
   public Guid AckRequestId { get; set; }
}