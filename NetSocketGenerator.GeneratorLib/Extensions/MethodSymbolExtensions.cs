
namespace NetSocketGenerator.GeneratorLib.Extensions;

public static class MethodSymbolExtensions
{
   public static MethodInfo? GetMethodInfo(this IMethodSymbol symbol)
   {
      var returnType = symbol.MethodKind switch
      {
         MethodKind.Constructor or MethodKind.StaticConstructor => null,
         _ when symbol.ReturnsVoid => "null",
         _ => symbol.ReturnType.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat),
      };

      var attributes = symbol.GetAttributes();
      
      return new MethodInfo(
         symbol.Name,
         returnType,
         attributes.GetAttributesInfo(),
         new EquatableArray<ParameterInfo>(symbol.Parameters.GetParameterInfos()));
   }
}