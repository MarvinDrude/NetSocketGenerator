
namespace NetSocketGenerator.GeneratorLib.Extensions;

public static class NamedTypeSymbolExtensions
{
   public static ClassInfo GetInfo(this INamedTypeSymbol symbol)
   {
      var nameSpace = symbol.ContainingNamespace.ToString();
      if (nameSpace == "<global namespace>")
      {
         nameSpace = null;
      }
      
      return new ClassInfo(
         symbol.Name,
         nameSpace,
         symbol.GetTypeName(),
         symbol.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat),
         symbol.GetModifiers(),
         symbol.IsGenericType,
         symbol.GetContainingNestedTypes());
   }

   public static ClassModifiers GetModifiers(this INamedTypeSymbol symbol)
   {
      var modifiers = ClassModifiers.None;

      if (symbol.IsSealed)
      {
         modifiers |= ClassModifiers.Sealed;
      }
      else if (symbol.IsStatic)
      {
         modifiers |= ClassModifiers.Static;
      }

      if (symbol.DeclaredAccessibility == Accessibility.Internal)
      {
         modifiers |= ClassModifiers.Internal;
      }

      if (symbol.Locations.Length > 1)
      {
         modifiers |= ClassModifiers.Partial;
      }
      
      return modifiers;
   }
   
   public static string GetTypeName(this INamedTypeSymbol symbol)
   {
      return symbol switch
      {
         { IsRecord: true, TypeKind: TypeKind.Struct } => "record struct",
         { IsRecord: true } => "record",
         { TypeKind: TypeKind.Interface } => "interface",
         { TypeKind: TypeKind.Struct } => "struct",

         _ => "class"
      };
   }

   public static EquatableArray<string> GetContainingNestedTypes(this INamedTypeSymbol? symbol)
   {
      List<string> nested = [];

      while (symbol is not null && symbol.Kind != SymbolKind.Namespace)
      {
         symbol = symbol.ContainingType;
         
         if (symbol is not null)
            nested.Add(symbol.Name);
      }
      
      return new EquatableArray<string>([.. nested]);
   }

   public static bool IsVoidTask(this INamedTypeSymbol symbol)
   {
      return symbol is {
         Name: "Task",
         ContainingNamespace:
         {
            Name: "Tasks",
            ContainingNamespace:
            {
               Name: "Threading",
               ContainingNamespace:
               {
                  Name: "System",
                  ContainingNamespace.IsGlobalNamespace: true
               }
            }
         }
      };
   }
}
