
namespace NetSocketGenerator.GeneratorLib.Extensions;

public static class ParameterSymbolExtensions
{
   public static ParameterInfo[] GetParameterInfos(this ImmutableArray<IParameterSymbol> symbols)
   {
      return [.. 
         symbols
            .Select(x => x.GetParameterInfo())
            .Where(x => x is not null)
            .Select(x => x!.Value)
      ];
   }
   
   public static ParameterInfo? GetParameterInfo(this IParameterSymbol parameter)
   {
      return new ParameterInfo(
         parameter.Name,
         parameter.Type.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat),
         parameter.GetAttributes().GetAttributesInfo(),
         null);
   }
}