namespace NetSocketGenerator.Generator.Processor;

[Generator]
public sealed partial class ProcessorGenerator : IIncrementalGenerator
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

      var maybeProcessorInfos = maybeProcessors
         .Combine(assemblyName);
      
      context.RegisterSourceOutput(
         maybeProcessorInfos,
         static (spc, node) => 
            Render(spc, node.Left));
   }
}