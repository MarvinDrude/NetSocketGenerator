
namespace NetSocketGenerator.GeneratorLib.Info;

public readonly record struct MethodInfo(
   string Name,
   string? ReturnTypeFullName,
   AttributesInfo Attributes,
   EquatableArray<ParameterInfo> Parameters)
{
   public readonly string Name = Name;
   public readonly string? ReturnTypeFullName = ReturnTypeFullName;

   public readonly AttributesInfo Attributes = Attributes;
   public readonly EquatableArray<ParameterInfo> Parameters = Parameters;

   public readonly bool IsConstructor = ReturnTypeFullName is null;
}