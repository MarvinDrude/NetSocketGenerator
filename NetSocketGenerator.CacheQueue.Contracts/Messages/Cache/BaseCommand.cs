using System.Text.Json.Serialization;

namespace NetSocketGenerator.CacheQueue.Contracts.Messages.Cache;

[JsonPolymorphic]
[JsonDerivedType(typeof(BaseCommand), "base")]
[JsonDerivedType(typeof(GetStringCommand), CommandNames.StringGet)]
[JsonDerivedType(typeof(SetStringCommand), CommandNames.StringSet)]
public class BaseCommand : MessageBase
{
   public required string KeyName { get; set; }

   public virtual string StoreType => "none";
}