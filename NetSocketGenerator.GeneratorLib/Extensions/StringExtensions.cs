namespace NetSocketGenerator.GeneratorLib.Extensions;

public static class StringExtensions
{
   public static string FirstCharToLower(this string str)
   {
      if (string.IsNullOrEmpty(str) || char.IsLower(str[0]))
      {
         return str;
      }
      
      return char.ToLowerInvariant(str[0]) + str[1..];
   }
   
   public static string FirstCharToUpper(this string str)
   {
      if (string.IsNullOrEmpty(str) || char.IsUpper(str[0]))
      {
         return str;
      }
      
      return char.ToUpperInvariant(str[0]) + str[1..];
   }
}