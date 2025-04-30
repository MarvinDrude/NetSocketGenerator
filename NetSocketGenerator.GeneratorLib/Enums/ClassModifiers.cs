namespace NetSocketGenerator.GeneratorLib.Enums;

[Flags]
public enum ClassModifiers
{
   None = 0,
   Static = 1 << 0,
   Sealed = 1 << 1,
   Internal = 1 << 2,
   Partial = 1 << 3,
}