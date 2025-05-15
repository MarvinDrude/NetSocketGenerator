namespace NetSocketGenerator.GeneratorLib.Writers.Modules;

public sealed class MethodModule(CodeWriter writer) 
   : BaseModule<MethodModule>(writer)
{
   private readonly CodeWriter _writer = writer;

   public MethodModule OpenHeader(
      string modifiers, string returnType, string name)
   {
      _writer.WriteLine($"{modifiers} {returnType} {name}(");
      return this;
   }

   public MethodModule AddFirstParameter(string type, string name)
   {
      _writer.UpIndent();
      _writer.Write($"{type} {name}");
      return this;
   }

   public MethodModule AddNextParameter(string type, string name)
   {
      _writer.WriteLine(",");
      _writer.Write($"{type} {name}");
      return this;
   }

   public MethodModule CloseHeader(bool semicolon = false)
   {
      _writer.WriteLine($"){(semicolon ? ";" : string.Empty)}");
      return this;
   }

   public CodeWriter Done()
   {
      _writer.CancellationToken.ThrowIfCancellationRequested();
      return _writer;
   }
}