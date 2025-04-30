namespace NetSocketGenerator.GeneratorLib.Extensions;

public static class AttributeDataExtensions
{
   public static AttributesInfo GetAttributesInfo(this ImmutableArray<AttributeData> attributes)
   {
      return new AttributesInfo(new EquatableArray<AttributeInfo>(
         attributes
            .Select(x => x.GetAttributeInfo())
            .Where(x => x is not null)
            .Select(x => x!.Value)
            .ToArray()
         )
      );
   }

   public static AttributeInfo? GetAttributeInfo(this AttributeData attribute)
   {
      if (attribute.AttributeClass is not { } classSymbol)
      {
         return null;
      }
      
      return new AttributeInfo(
         classSymbol.Name,
         classSymbol.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat));
   }
   
   public static TypedConstant? GetNamedArgument(
      this AttributeData attributeData, string argumentName)
   {
      foreach (var argument in attributeData.NamedArguments)
      {
         if (argument.Key == argumentName)
         {
            return argument.Value;
         }
      }

      return null;
   }
}