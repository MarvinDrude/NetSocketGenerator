using System.Text.Json.Serialization;

namespace NetSocketGenerator.CacheQueue.Contracts.Messages.Cache;

[JsonPolymorphic(TypeDiscriminatorPropertyName = "commandType")]
[JsonDerivedType(typeof(GetStringCommand), CommandNames.StringGet)]
[JsonDerivedType(typeof(SetStringCommand), CommandNames.StringSet)]
public abstract class BaseCommand : MessageBase
{
   public required string KeyName { get; set; }
   
   public abstract string StoreType { get; }
}