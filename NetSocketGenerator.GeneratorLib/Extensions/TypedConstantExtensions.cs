
namespace NetSocketGenerator.GeneratorLib.Extensions;

public static class TypedConstantExtensions
{
   public static IEnumerable<string> GetEnumFullValues(this TypedConstant typedConstant)
   {
      if (typedConstant.Kind is not TypedConstantKind.Array)
      {
         yield break;
      }

      foreach (var enumValue in typedConstant.Values)
      {
         if (enumValue.GetEnumFullValue() is { } value)
         {
            yield return value;
         }
      }
   }
   
   public static string? GetEnumFullValue(this TypedConstant typedConstant)
   {
      return typedConstant.Value is int
         ? typedConstant.ToCSharpString()
         : null;
   }
   
   public static IEnumerable<INamedTypeSymbol> GetOriginalTypeSymbols(this TypedConstant typedConstant)
   {
      if (typedConstant.Type is not IArrayTypeSymbol
          {
             ElementType:
             {
                Name: "Type",
                ContainingNamespace:
                {
                   Name: "System",
                   ContainingNamespace.IsGlobalNamespace: true
                }
             }
          })
      {
         yield break;
      }
      
      foreach (var constant in typedConstant.Values)
      {
         if (constant.GetOriginalTypeSymbol() is not { } originalTypeSymbol)
         {
            continue;
         }  
         
         yield return originalTypeSymbol;
      }
   }

   public static INamedTypeSymbol? GetOriginalTypeSymbol(this TypedConstant typedConstant)
   {
      return typedConstant.Value is not INamedTypeSymbol symbol 
         ? null 
         : symbol.OriginalDefinition;
   }
}