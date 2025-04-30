namespace NetSocketGenerator.GeneratorLib.Extensions;

public static class SymbolExtensions
{
   public static MethodInfo? GetMethodInfo(this ImmutableArray<ISymbol> symbols, Func<IMethodSymbol, bool> predicate)
   {
      return symbols.OfType<IMethodSymbol>()
         .Where(predicate)
         .FirstOrDefault() is not { } method
         ? null 
         : method.GetMethodInfo();
   }
}