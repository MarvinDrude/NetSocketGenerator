namespace NetSocketGenerator.Generator.Processor;

public sealed partial class ProcessorGenerator
{
   private static void Render(
      SourceProductionContext context,
      MaybeInfo<ProcessorInfo> maybeProcessorInfo)
   {
      context.DispatchDiagnostics(Diagnostics, maybeProcessorInfo);

      if (!maybeProcessorInfo.HasValue)
      {
         return;
      }
      
      var processor = maybeProcessorInfo.Value;
      
      var token = context.CancellationToken;
      using var cw = new CodeWriter();

      token.ThrowIfCancellationRequested();
      
      cw.WriteLine("#nullable enable");
      cw.WriteLine();
      
      cw.WriteLine("using System;");
      cw.WriteLine("using NetSocketGenerator.Events.Delegates;");
      cw.WriteLine("using NetSocketGenerator.Tcp.Interfaces;");
      cw.WriteLine("using NetSocketGenerator.Extensions;");
      
      if(processor.ClassInfo.NameSpace is { } nameSpace)
      {
         cw.WriteLine();
         cw.WriteLine($"namespace {nameSpace};");
      }
      cw.WriteLine();
      
      cw.WriteLine(processor.ClassInfo.GetClassString());
      cw.WriteLine($"{{");
      cw.UpIndent();
      
      token.ThrowIfCancellationRequested();
      
      cw.WriteLine("public ServerFrameMessageHandler GetExecuteMethod(bool isServer)");
      cw.WriteLine($"{{");
      cw.UpIndent();
      
      cw.WriteLine($"return ExecuteInner;");
      cw.WriteLine();
      
      RenderExecuteInner(cw, processor.MethodInfo, token);
      
      cw.DownIndent();
      cw.WriteLine($"}}");
      
      cw.DownIndent();
      cw.WriteLine($"}}");
      
      token.ThrowIfCancellationRequested();
      context.AddSource($"{processor.ClassInfo.NameSpace ?? "Global"}.{processor.ClassInfo.Name}.g.cs", cw.ToString());
   }

   private static void RenderExecuteInner(
      CodeWriter cw,
      MethodInfo methodInfo,
      CancellationToken token)
   {
      cw.WriteLine("async Task ExecuteInner(ITcpConnection connection, string? id, ReadOnlyMemory<byte> payload)");
      cw.WriteLine($"{{");
      cw.UpIndent();
      
      token.ThrowIfCancellationRequested();

      var hasServices = false;
      
      List<string> parameterExpressions = [];
      List<Action> writeExpressions = [];
      foreach (var parameter in methodInfo.Parameters)
      {
         if (HandleExecuteParameter(cw, parameter, writeExpressions, parameterExpressions))
         {
            hasServices = true;
         }
      }

      
      if (hasServices)
      {
         cw.WriteLine("var tcpServices = (ITcpServices)connection;");
         cw.WriteLine("using var serviceScope = tcpServices.CreateTcpScope();");
         cw.WriteLine("var serviceProvider = serviceScope.ServiceProvider;");
         cw.WriteLine();
      }

      foreach (var expression in writeExpressions)
      {
         expression();
      }
      
      cw.WriteLine();
      cw.WriteLine("await Execute(");
      cw.UpIndent();
      
      for (var index = 0; index < parameterExpressions.Count; index++)
      {
         var parameterExpression = parameterExpressions[index];
         
         cw.Write(parameterExpression);

         if (index != parameterExpressions.Count - 1)
         {
            cw.WriteLine(",");
         }
      }

      cw.WriteLine(");");
      cw.DownIndent();
      
      cw.DownIndent();
      cw.WriteLine($"}}");
   }
   
   private static bool HandleExecuteParameter(
      CodeWriter cw,
      ParameterInfo parameterInfo,
      List<Action> writeExpressions,
      List<string> parameterExpressions)
   {
      return parameterInfo.FullTypeName switch
      {
         "global::NetSocketGenerator.Tcp.Interfaces.ITcpConnection" 
            => WriteParameter(writeExpressions, () => WriteConnection(cw, parameterInfo, parameterExpressions), false),
         "global::NetSocketGenerator.Tcp.Interfaces.ITcpServer" 
            => WriteParameter(writeExpressions, () => WriteServerConnection(cw, parameterInfo, parameterExpressions), false),
         "global::NetSocketGenerator.Tcp.Interfaces.ITcpClient" 
            => WriteParameter(writeExpressions, () => WriteClientConnection(cw, parameterInfo, parameterExpressions), false),
         
         _ when parameterInfo.Attributes.Attributes.Any(x => x.FullTypeName == AttributeSocketPayload) 
            => WriteParameter(writeExpressions, () => WritePayload(cw, parameterInfo, parameterExpressions), false),
         
         _ when parameterInfo.Attributes.Attributes.Any(x => x.FullTypeName == AttributeSocketEventName) 
            => WriteParameter(writeExpressions, () => WriteEventName(cw, parameterInfo, parameterExpressions), false),
            
         _ => WriteParameter(writeExpressions, () => WriteServiceParameter(cw, parameterInfo, parameterExpressions), true),
      };
   }

   private static void WritePayload(
      CodeWriter cw,
      ParameterInfo parameter,
      List<string> parameterExpressions)
   {
      if (parameter.FullTypeName == "global::System.ReadOnlyMemory<byte>")
      {
         parameterExpressions.Add("payload");
         return;
      }

      cw.WriteLine();
      cw.WriteLine($"var deserialized = connection.Serializer.Deserialize<{parameter.FullTypeName}>(payload.Span);");
      cw.WriteLine("if (deserialized is null)");
      cw.WriteLine($"{{");
      cw.UpIndent();
      cw.WriteLine("return;");
      cw.DownIndent();
      cw.WriteLine($"}}");
      
      parameterExpressions.Add("deserialized");
   }
   
   private static void WriteServerConnection(
      CodeWriter cw,
      ParameterInfo parameter,
      List<string> parameterExpressions)
   {
      parameterExpressions.Add("(ITcpServer)connection");
   }
   private static void WriteClientConnection(
      CodeWriter cw,
      ParameterInfo parameter,
      List<string> parameterExpressions)
   {
      parameterExpressions.Add("(ITcpClient)connection");
   }
   
   private static void WriteConnection(
      CodeWriter cw,
      ParameterInfo parameter,
      List<string> parameterExpressions)
   {
      parameterExpressions.Add("connection");
   }
   
   private static void WriteEventName(
      CodeWriter cw,
      ParameterInfo parameter,
      List<string> parameterExpressions)
   {
      parameterExpressions.Add("id!");
   }

   private static void WriteServiceParameter(
      CodeWriter cw,
      ParameterInfo parameter,
      List<string> parameterExpressions)
   {
      var variableName = $"_pr{parameter.Name.FirstCharToUpper()}";
      parameterExpressions.Add(variableName);
      
      cw.WriteLine($"var {variableName} = serviceProvider.GetRequiredServiceObject<{parameter.FullTypeName}>();");
   }

   private static bool WriteParameter(
      List<Action> writeExpressions,
      Action action,
      bool isService)
   {
      writeExpressions.Add(action);
      return isService;
   }
}