namespace NetSocketGenerator.GeneratorLib.Builders;

public sealed class DiagnosticBuilder<T>
   where T : new()
{
   private readonly List<DiagnosticInfo> _diagnostics = [];
   
   public static DiagnosticBuilder<T> Create()
   {
      return new DiagnosticBuilder<T>();
   }

   public static MaybeInfo<T> CreateSingle(string diagnosticId, params string[] arguments)
   {
      return Create()
         .Add(diagnosticId, arguments)
         .Build();
   }

   private DiagnosticBuilder()
   {
      
   }

   public DiagnosticBuilder<T> Add(string diagnosticId, params string[] arguments)
   {
      _diagnostics.Add(new DiagnosticInfo(diagnosticId, new EquatableArray<string>(arguments)));
      return this;
   }

   public MaybeInfo<T> Build()
   {
      return new MaybeInfo<T>(false, new T(), new EquatableArray<DiagnosticInfo>([.. _diagnostics]));
   }

   public MaybeInfo<T> Build(T value)
   {
      return new MaybeInfo<T>(true, value, new EquatableArray<DiagnosticInfo>([.. _diagnostics]));
   }
}