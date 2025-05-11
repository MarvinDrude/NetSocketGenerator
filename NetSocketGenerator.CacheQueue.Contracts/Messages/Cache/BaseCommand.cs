using System.Text.Json.Serialization;

namespace NetSocketGenerator.CacheQueue.Contracts.Messages.Cache;

[JsonPolymorphic]
[JsonDerivedType(typeof(BaseCommand), "base")]
[JsonDerivedType(typeof(DeleteCommand), CommandNames.Delete)]
[JsonDerivedType(typeof(BatchCommand), CommandNames.Batch)]
[JsonDerivedType(typeof(GetStringCommand), CommandNames.StringGet)]
[JsonDerivedType(typeof(SetStringCommand), CommandNames.StringSet)]
[JsonDerivedType(typeof(GetDoubleCommand), CommandNames.DoubleGet)]
[JsonDerivedType(typeof(SetDoubleCommand), CommandNames.DoubleSet)]
[JsonDerivedType(typeof(AddDoubleCommand), CommandNames.DoubleAdd)]
[JsonDerivedType(typeof(GetIntegerCommand), CommandNames.IntegerGet)]
[JsonDerivedType(typeof(SetIntegerCommand), CommandNames.IntegerSet)]
[JsonDerivedType(typeof(AddIntegerCommand), CommandNames.IntegerAdd)]
[JsonDerivedType(typeof(GetLongCommand), CommandNames.LongGet)]
[JsonDerivedType(typeof(SetLongCommand), CommandNames.LongSet)]
[JsonDerivedType(typeof(AddLongCommand), CommandNames.LongAdd)]
[JsonDerivedType(typeof(GetULongCommand), CommandNames.ULongGet)]
[JsonDerivedType(typeof(SetULongCommand), CommandNames.ULongSet)]
[JsonDerivedType(typeof(AddULongCommand), CommandNames.ULongAdd)]
public class BaseCommand : MessageBase
{
   public required string KeyName { get; set; }

   public virtual string StoreType => "none";

   public override string ToString()
   {
      return $"[STORE_{StoreType}][KEY_{KeyName}]";
   }
}