namespace NetSocketGenerator.Generator.Processor;

public sealed partial class ProcessorGenerator
{
   private readonly record struct ProcessorInfo(
      ClassInfo ClassInfo,
      MethodInfo MethodInfo,
      string EventNamePattern,
      bool IncludeServer,
      bool IncludeClient)
   {
      public readonly ClassInfo ClassInfo = ClassInfo;
      public readonly MethodInfo MethodInfo = MethodInfo;
      
      public readonly string EventNamePattern = EventNamePattern;
      public readonly bool IncludeServer = IncludeServer;
      public readonly bool IncludeClient = IncludeClient;
   }
}