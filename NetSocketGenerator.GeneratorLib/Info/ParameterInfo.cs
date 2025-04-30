namespace NetSocketGenerator.GeneratorLib.Info;

public readonly record struct ParameterInfo(
   string Name,
   string FullTypeName,
   AttributesInfo Attributes,
   string? ValueAsString)
{
   public readonly string Name = Name;
   public readonly string FullTypeName = FullTypeName;

   public readonly AttributesInfo Attributes = Attributes;
   public readonly string? ValueAsString = ValueAsString;
}