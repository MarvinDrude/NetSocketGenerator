namespace NetSocketGenerator.GeneratorLib.Writers.Modules;

public class BaseModule<TSelf>(CodeWriter writer)
   where TSelf : BaseModule<TSelf>
{
   public TSelf OpenBody()
   {
      writer.WriteLine($"{{");
      writer.UpIndent();
      return (TSelf)this;
   }

   public TSelf CloseBody()
   {
      writer.DownIndent();
      writer.WriteLine($"}}");
      return (TSelf)this;
   }
   
   public TSelf UpIndent()
   {
      writer.UpIndent();
      return (TSelf)this;
   }
   
   public TSelf DownIndent()
   {
      writer.DownIndent();
      return (TSelf)this;
   }
   
   public TSelf WriteLine(string line)
   {
      writer.WriteLine(line, false);
      return (TSelf)this;
   }

   public TSelf WriteLine()
   {
      writer.WriteLine();
      return (TSelf)this;
   }
   
   public TSelf Write(string line)
   {
      writer.Write(line, false);
      return (TSelf)this;
   }
}