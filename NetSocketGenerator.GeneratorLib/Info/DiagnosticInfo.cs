namespace NetSocketGenerator.GeneratorLib.Info;

public readonly record struct MaybeInfo<T>(
   T? Value,
   EquatableArray<DiagnosticInfo> Diagnostics = default)
{
   public readonly T? Value = Value;
   public readonly EquatableArray<DiagnosticInfo> Diagnostics = Diagnostics;
}

public readonly record struct DiagnosticInfo(
   string DiagnosticId,
   EquatableArray<string> Arguments = default)
{
   public readonly string DiagnosticId = DiagnosticId;
   public readonly EquatableArray<string> Arguments = Arguments;
}