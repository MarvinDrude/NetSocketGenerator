
namespace NetSocketGenerator.GeneratorLib.Writers;

public sealed class CodeWriter : IDisposable
{
   private const string DefaultIndent = "\t";
   private const string DefaultNewLine = "\n";
   
   public CancellationToken CancellationToken { get; }
   
   public NameSpaceModule NameSpace { get; }
   public ClassModule Class { get; }
   public MethodModule Method { get; }

   private string Indent { get; set; } = DefaultIndent;
   private string NewLine { get; set; } = DefaultNewLine;

   private string[] _levelCache;

   private ArrayBuilder<char> Builder { get; set; } = new ArrayBuilder<char>();
   private int CurrentLevel { get; set; } = 0;
   private string CurrentLevelString { get; set; } = "";

   public CodeWriter(CancellationToken token = default)
   {
      CancellationToken = token;
      
      _levelCache = new string[6];
      _levelCache[0] = "";

      for (var e = 1; e < _levelCache.Length; e++)
      {
         _levelCache[e] = _levelCache[e - 1] + Indent;
      }

      NameSpace = new NameSpaceModule(this);
      Class = new ClassModule(this);
      Method = new MethodModule(this);
   }

   public void UpIndent()
   {
      CurrentLevel++;
      if (CurrentLevel == _levelCache.Length)
      {
         Array.Resize(ref _levelCache, _levelCache.Length * 2);
      }

      CurrentLevelString = _levelCache[CurrentLevel]
         ??= _levelCache[CurrentLevel - 1] + Indent;
   }

   public void DownIndent()
   {
      CurrentLevel--;
      CurrentLevelString = _levelCache[CurrentLevel];
   }

   public Span<char> Advance(int size)
   {
      AddIndentOnDemand();
      return Builder.Advance(size);
   }

   public void WriteText(string text)
      => WriteText(text.AsSpan());

   public void WriteText(ReadOnlySpan<char> text)
   {
      AddIndentOnDemand();
      Builder.AddRange(text);
   }

   public void Write(string text, bool multiLine = false)
      => Write(text.AsSpan(), multiLine);

   public void Write(ReadOnlySpan<char> content, bool multiLine = false)
   {
      if (!multiLine)
      {
         WriteText(content);
      }
      else
      {
         while (content.Length > 0)
         {
            var newLinePosition = content.IndexOf(NewLine[0]);

            if (newLinePosition >= 0)
            {
               var line = content[..newLinePosition];

               WriteIf(!line.IsEmpty, line);
               WriteLine();

               content = content[(newLinePosition + 1)..];
            }
            else
            {
               WriteText(content);
               break;
            }
         }
      }
   }

   public void WriteIf(bool condition, string content, bool multiLine = false)
      => WriteIf(condition, content.AsSpan(), multiLine);

   public void WriteIf(bool condition, ReadOnlySpan<char> content, bool multiLine = false)
   {
      if (condition)
      {
         Write(content, multiLine);
      }
   }

   public void WriteLine(string content, bool multiLine = false)
      => WriteLine(content.AsSpan(), multiLine);

   public void WriteLine(ReadOnlySpan<char> content, bool multiLine = false)
   {
      Write(content, multiLine);
      WriteLine();
   }

   public void WriteLineIf(bool condition)
   {
      if (condition)
      {
         WriteLine();
      }
   }
   
   public void WriteLineIf(bool condition, string content, bool multiLine = false)
      => WriteLineIf(condition, content.AsSpan(), multiLine);

   public void WriteLineIf(bool condition, ReadOnlySpan<char> content, bool multiLine = false)
   {
      if (condition)
      {
         WriteLine(content, multiLine);
      }
   }

   public void WriteLine()
   {
      Builder.Add(NewLine[0]);
   }

   public override string ToString()
   {
      return Builder.Span.Trim().ToString();
   }

   public void Dispose()
   {
      Builder.Dispose();
   }

   private void AddIndentOnDemand()
   {
      if (Builder.Count == 0 || Builder.Span[^1] == NewLine[0])
      {
         Builder.AddRange(CurrentLevelString.AsSpan());
      }
   }
}