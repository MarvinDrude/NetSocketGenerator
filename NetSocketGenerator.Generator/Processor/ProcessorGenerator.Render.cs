namespace NetSocketGenerator.Generator.Processor;

public sealed partial class ProcessorGenerator
{
   private static void Render(
      SourceProductionContext context,
      MaybeInfo<ProcessorInfo> maybeProcessorInfo)
   {
      context.DispatchDiagnostics(Diagnostics, maybeProcessorInfo);

      if (!maybeProcessorInfo.HasValue)
      {
         return;
      }
      
      var processor = maybeProcessorInfo.Value;
      
      var token = context.CancellationToken;
      using var cw = new CodeWriter();

      token.ThrowIfCancellationRequested();
      
      cw.WriteLine("#nullable enable");
      cw.WriteLine();
      
      cw.WriteLine("using System;");
      
      if(processor.ClassInfo.NameSpace is { } nameSpace)
      {
         cw.WriteLine();
         cw.WriteLine($"namespace {nameSpace};");
      }
      cw.WriteLine();
      
      
      
      token.ThrowIfCancellationRequested();
      context.AddSource($"{processor.ClassInfo.NameSpace ?? "Global"}.{processor.ClassInfo.Name}.g.cs", cw.ToString());
   }
}