namespace NetSocketGenerator.GeneratorLib.Info;

public readonly record struct MaybeInfo<T>(
   bool HasValue,
   T Value,
   EquatableArray<DiagnosticInfo> Diagnostics)
{
   public readonly bool HasValue = HasValue;
   public readonly T Value = Value;
   public readonly EquatableArray<DiagnosticInfo> Diagnostics = Diagnostics;
}

public readonly record struct DiagnosticInfo(
   string DiagnosticId,
   EquatableArray<string> Arguments)
{
   public readonly string DiagnosticId = DiagnosticId;
   public readonly EquatableArray<string> Arguments = Arguments;
}