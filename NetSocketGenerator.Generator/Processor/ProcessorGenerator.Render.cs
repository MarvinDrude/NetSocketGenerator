namespace NetSocketGenerator.Generator.Processor;

public sealed partial class ProcessorGenerator
{
   private static void Render(
      SourceProductionContext context,
      MaybeProcessorInfo maybeProcessorInfo)
   {
      if (maybeProcessorInfo.DiagnosticIds.Count > 0)
      {
         foreach (var diagnosticId in maybeProcessorInfo.DiagnosticIds)
         {
            var descriptor = Diagnostics[diagnosticId];
            context.ReportDiagnostic(Diagnostic.Create(descriptor, Location.None));
         }
      }
      
      
   }
}