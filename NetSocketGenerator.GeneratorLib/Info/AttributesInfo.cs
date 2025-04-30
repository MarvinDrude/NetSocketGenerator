namespace NetSocketGenerator.GeneratorLib.Info;

public readonly record struct AttributesInfo(
   EquatableArray<AttributeInfo> Attributes)
{
   public readonly EquatableArray<AttributeInfo> Attributes = Attributes;
}

public readonly record struct AttributeInfo(
   string Name,
   string FullTypeName)
{
   public readonly string Name = Name;
   public readonly string FullTypeName = FullTypeName;
}