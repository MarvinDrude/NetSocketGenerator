namespace NetSocketGenerator.GeneratorLib.Extensions;

public static class SourceProductionContextExtensions
{
   public static void DispatchDiagnostics<T>(
      this SourceProductionContext context, 
      Dictionary<string, DiagnosticDescriptor> descriptors, 
      MaybeInfo<T> maybeInfo)
   {
      if (maybeInfo.Diagnostics.Count == 0)
      {
         return;
      }

      foreach (var diagnostic in maybeInfo.Diagnostics)
      {
         if (!descriptors.TryGetValue(diagnostic.DiagnosticId, out var descriptor))
         {
            continue;
         }

         var args = diagnostic.Arguments.Cast<object>().ToArray();
         context.ReportDiagnostic(Diagnostic.Create(descriptor, Location.None, args));
      }
   }
}