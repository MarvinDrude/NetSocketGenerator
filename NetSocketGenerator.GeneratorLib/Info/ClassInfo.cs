
namespace NetSocketGenerator.GeneratorLib.Info;

public readonly record struct ClassInfo(
   string Name,
   string? NameSpace,
   string Type,
   string FullTypeName,
   ClassModifiers Modifiers,
   bool IsGeneric,
   EquatableArray<string> NestedTypes)
{
   public readonly string Name = Name;
   public readonly string? NameSpace  = NameSpace;
   
   public readonly string Type = Type;
   public readonly string FullTypeName = FullTypeName;
   
   public readonly ClassModifiers Modifiers = Modifiers;
   public readonly bool IsGeneric = IsGeneric;

   public readonly EquatableArray<string> NestedTypes = NestedTypes;

   public string? GetNamespaceIncludingNestedTypes()
   {
      List<string> parts = [.. NestedTypes];
      
      if (NameSpace is not null)
      {
         parts.Insert(0, NameSpace);
      }

      return parts.Count == 0 
         ? null 
         : string.Join(".", parts);
   }
   
   public string GetClassString(bool forcePartial = true)
   {
      List<string> parts = [
         Modifiers.HasFlag(ClassModifiers.Internal) ? "internal" : "public"
      ];

      if (Modifiers.HasFlag(ClassModifiers.Sealed))
      {
         parts.Add("sealed");
      }
      else if (Modifiers.HasFlag(ClassModifiers.Static))
      {
         parts.Add("static");
      }

      if (forcePartial || Modifiers.HasFlag(ClassModifiers.Partial))
      {
         parts.Add("partial");
      }

      parts.Add(Type);
      parts.Add(Name);
      
      return string.Join(" ", parts);
   }
}