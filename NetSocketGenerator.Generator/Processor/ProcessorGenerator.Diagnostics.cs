namespace NetSocketGenerator.Generator.Processor;

public sealed partial class ProcessorGenerator
{
   private static readonly DiagnosticDescriptor InvalidProcessorClassRule = new DiagnosticDescriptor(
      id: "NSG001",
      title: "Invalid [SocketProcessor] usage",
      messageFormat: "Invalid class for being decorated with [SocketProcessor]",
      category: "ProcessorGenerator",
      defaultSeverity: DiagnosticSeverity.Warning,
      isEnabledByDefault: true);
   
   private static readonly Dictionary<string, DiagnosticDescriptor> Diagnostics = new()
   {
      ["NSG001"] = InvalidProcessorClassRule,
   };
}