namespace NetSocketGenerator.GeneratorLib.Writers.Modules;

public sealed class ClassModule(CodeWriter writer) 
   : BaseModule<ClassModule>(writer)
{
   private readonly CodeWriter _writer = writer;
   
   public ClassModule Declaration(ClassInfo classInfo, bool forcePartial = true)
   {
      _writer.WriteLine(classInfo.GetClassString());
      return this;
   }

   public ClassModule FirstBaseDeclaration(string baseClassName, bool close = false)
   {
      _writer.UpIndent();
      _writer.Write($" : {baseClassName}");

      return close 
         ? CloseBaseDeclaration() 
         : this;
   }

   public ClassModule NextBaseDeclaration(string baseClassName, bool close = false)
   {
      _writer.WriteLine(",");
      _writer.Write($"  {baseClassName}");
      
      return close 
         ? CloseBaseDeclaration() 
         : this;
   }
   
   public ClassModule CloseBaseDeclaration()
   {
      _writer.WriteLine();
      _writer.DownIndent();
      return this;
   }

   public CodeWriter Done()
   {
      _writer.CancellationToken.ThrowIfCancellationRequested();
      return _writer;
   }
}