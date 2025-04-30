namespace NetSocketGenerator.GeneratorLib.Extensions;

public static class TypeSymbolExtensions
{
   public static bool IsVoidTask(this ITypeSymbol type)
   {
      return type is INamedTypeSymbol typeSymbol
             && typeSymbol.IsVoidTask();
   }
}