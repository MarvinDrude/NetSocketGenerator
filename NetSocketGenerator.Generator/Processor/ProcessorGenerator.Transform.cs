
namespace NetSocketGenerator.Generator.Processor;

public sealed partial class ProcessorGenerator
{
   private static MaybeInfo<ProcessorInfo>? Transform(
      GeneratorAttributeSyntaxContext context,
      CancellationToken token)
   {
      token.ThrowIfCancellationRequested();

      var symbol = (INamedTypeSymbol)context.TargetSymbol;
      var classInfo = symbol.GetInfo();
      
      token.ThrowIfCancellationRequested();
      
      var attributes = symbol.GetAttributes();
      if (GetProcessorAttribute(attributes) is not { } processorAttribute)
      {
         return null;
      }

      if (processorAttribute.GetStringNamedArgument("EventNamePattern") is not { } eventNamePattern)
      {
         return DiagnosticBuilder<ProcessorInfo>.CreateSingle("NSG001", classInfo.FullTypeName);
      }
      
      token.ThrowIfCancellationRequested();

      if (symbol.GetMembers().GetMethodInfo(IsExecuteMethod) is not { } methodInfo)
      {
         return DiagnosticBuilder<ProcessorInfo>.CreateSingle("NSG001", classInfo.FullTypeName);
      }
      
      token.ThrowIfCancellationRequested();
      
      return new MaybeInfo<ProcessorInfo>(
         true,
         new ProcessorInfo(
            classInfo, 
            methodInfo,
            eventNamePattern,
            processorAttribute.GetBooleanNamedArgument("IncludeServer", true),
            processorAttribute.GetBooleanNamedArgument("IncludeClient", true)), 
         []);
   }
   
   private static bool IsExecuteMethod(IMethodSymbol methodSymbol)
   {
      if (methodSymbol.Name != "Execute" || methodSymbol.IsStatic)
      {
         return false;
      }

      if (methodSymbol.ReturnsByRef 
          || methodSymbol.ReturnsVoid
          || methodSymbol.ReturnsByRefReadonly)
      {
         return false;
      }
      
      return methodSymbol.ReturnType.IsVoidTask();
   }
}