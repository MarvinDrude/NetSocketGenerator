using System.Text.Json.Serialization;

namespace NetSocketGenerator.CacheQueue.Contracts.Messages;

[JsonPolymorphic]
[JsonDerivedType(typeof(AckMessageBase), "base")]
[JsonDerivedType(typeof(DeleteCommandAck), CommandNames.Delete)]
[JsonDerivedType(typeof(SetStringCommandAck), CommandNames.StringSet)]
[JsonDerivedType(typeof(GetStringCommandAck), CommandNames.StringGet)]
[JsonDerivedType(typeof(GetDoubleCommandAck), CommandNames.DoubleGet)]
[JsonDerivedType(typeof(SetDoubleCommandAck), CommandNames.DoubleSet)]
[JsonDerivedType(typeof(GetIntegerCommandAck), CommandNames.IntegerGet)]
[JsonDerivedType(typeof(SetIntegerCommandAck), CommandNames.IntegerSet)]
[JsonDerivedType(typeof(GetLongCommandAck), CommandNames.LongGet)]
[JsonDerivedType(typeof(SetLongCommandAck), CommandNames.LongSet)]
[JsonDerivedType(typeof(GetULongCommandAck), CommandNames.ULongGet)]
[JsonDerivedType(typeof(SetULongCommandAck), CommandNames.ULongSet)]
public class AckMessageBase
{
   public Guid RequestId { get; } = Guid.CreateVersion7();
   
   public Guid AckRequestId { get; set; }
}