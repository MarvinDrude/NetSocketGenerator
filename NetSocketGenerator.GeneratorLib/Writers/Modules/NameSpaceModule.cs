namespace NetSocketGenerator.GeneratorLib.Writers.Modules;

public sealed class NameSpaceModule(CodeWriter writer)
   : BaseModule<NameSpaceModule>(writer)
{
   private readonly CodeWriter _writer = writer;

   public NameSpaceModule EnableNullable(bool extraLine = true)
   {
      _writer.WriteLine("#nullable enable");
      _writer.WriteLineIf(extraLine);
      return this;
   }
   
   public NameSpaceModule Using(string nameSpace)
   {
      _writer.WriteLine($"using {nameSpace};");
      return this;
   }

   public NameSpaceModule Set(string nameSpace, bool extraLine = true)
   {
      _writer.WriteLine($"namespace {nameSpace};");
      _writer.WriteLineIf(extraLine);
      return this;
   }

   public NameSpaceModule Set(ClassInfo classInfo, bool extraLine = true)
   {
      if (classInfo.NameSpace is { } nameSpace)
      {
         Set(nameSpace, extraLine);
      }
      return this;
   }

   public NameSpaceModule ExtraLine()
   {
      _writer.WriteLine();
      return this;
   }

   public CodeWriter Done()
   {
      _writer.CancellationToken.ThrowIfCancellationRequested();
      return _writer;
   }
}