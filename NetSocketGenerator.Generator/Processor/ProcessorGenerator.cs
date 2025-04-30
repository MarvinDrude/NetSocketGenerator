namespace NetSocketGenerator.Generator.Processor;

[Generator]
public sealed partial class ProcessorGenerator
{
   public void Initialize(IncrementalGeneratorInitializationContext context)
   {
      var assemblyName = context.CompilationProvider
         .Select(static (c, _) => c.AssemblyName!
            .Replace(" ", string.Empty)
            .Replace(".", string.Empty)
            .Trim());

      var maybeProcessors = context.SyntaxProvider
         .ForAttributeWithMetadataName(
            AttributeProcessorFullName,
            predicate: static (_, _) => true,
            transform: Transform
         )
         .Where(static e => e is not null)
         .Select(static (x, _) => x!.Value);

      
   }
}