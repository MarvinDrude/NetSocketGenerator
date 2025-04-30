namespace NetSocketGenerator.Generator.Processor;

public sealed partial class ProcessorGenerator
{
   private const string ParameterAttributesNameSpace = "global::NetSocketGenerator.Attributes";
   private const string AttributeSocketEventName = $"{ParameterAttributesNameSpace}.SocketEventNameAttribute";
   private const string AttributeSocketPayload = $"{ParameterAttributesNameSpace}.SocketPayloadAttribute";

   private const string AttributesNameSpace = "NetSocketGenerator.Attributes";
   private const string AttributeProcessorFullName = $"{AttributesNameSpace}.ProcessorAttribute";

   private static AttributeData? GetProcessorAttribute(ImmutableArray<AttributeData> attributes)
   {
      return attributes.FirstOrDefault(IsProcessorAttribute);
   }
   
   private static bool IsProcessorAttribute(AttributeData attributeData)
   {
      return attributeData.AttributeClass is
      {
         Name: "ProcessorAttribute"
      } && IsRelevantAttribute(attributeData);
   }
   
   private static bool IsRelevantAttribute(AttributeData attributeData)
   {
      return attributeData.AttributeClass is
      {
         ContainingNamespace:
         {
            Name: "Attributes",
            ContainingNamespace:
            {
               Name: "NetSocketGenerator",
               ContainingNamespace.IsGlobalNamespace: true
            }
         }
      };
   }
}