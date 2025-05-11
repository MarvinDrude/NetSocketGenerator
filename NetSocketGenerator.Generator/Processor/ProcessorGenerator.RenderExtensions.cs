namespace NetSocketGenerator.Generator.Processor;

public sealed partial class ProcessorGenerator
{
   private static void RenderExtensions(
      SourceProductionContext context,
      ImmutableArray<ProcessorInfo> processorInfos)
   {
      var token = context.CancellationToken;
      using var cw = new CodeWriter();

      token.ThrowIfCancellationRequested();
      
      cw.WriteLine("#nullable enable");
      cw.WriteLine();
      
      cw.WriteLine("using System;");
      cw.WriteLine("using NetSocketGenerator.Events.Delegates;");
      cw.WriteLine("using NetSocketGenerator.Tcp.Interfaces;");
      cw.WriteLine("using NetSocketGenerator.Extensions;");
      cw.WriteLine("using Microsoft.Extensions.DependencyInjection;");
      cw.WriteLine();

      cw.WriteLine("namespace NetSocketGenerator.Extensions;");
      cw.WriteLine();

      var groups = GroupByRegistrationGroups(processorInfos);
      
      cw.WriteLine("internal static partial class ServiceCollectionExtensions");
      cw.WriteLine($"{{");
      cw.UpIndent();

      foreach (var keypair in groups)
      {
         cw.WriteLine($"public static IServiceCollection AddSocket{keypair.Key}Processors(this IServiceCollection services)");
         cw.WriteLine($"{{");
         cw.UpIndent();

         foreach (var processorInfo in keypair.Value)
         {
            cw.WriteLine($"services.AddSingleton<{processorInfo.ClassInfo.FullTypeName}>();");
            token.ThrowIfCancellationRequested();
         }
         
         cw.WriteLine($"return services;");
         cw.DownIndent();
         cw.WriteLine($"}}");
      }

      cw.WriteLine();
      
      cw.DownIndent();
      cw.WriteLine($"}}");
      cw.WriteLine();
      
      cw.WriteLine("internal static partial class TcpServerClientExtensions");
      cw.WriteLine($"{{");
      cw.UpIndent();

      foreach (var keypair in groups)
      {
         if (keypair.Key.StartsWith("Client"))
         {
            cw.WriteLine($"public static ITcpClient UseSocket{keypair.Key}Processors(this ITcpClient client)");
            cw.WriteLine($"{{");
            cw.UpIndent();

            foreach (var processorInfo in keypair.Value)
            {
               cw.WriteLine($"client.UseKeyHandler<{processorInfo.ClassInfo.FullTypeName}>();");
               token.ThrowIfCancellationRequested();
            }
         
            cw.WriteLine($"return client;");
            cw.DownIndent();
            cw.WriteLine($"}}");
         }

         if (keypair.Key.StartsWith("Server"))
         {
            cw.WriteLine($"public static ITcpServer UseSocket{keypair.Key}Processors(this ITcpServer server)");
            cw.WriteLine($"{{");
            cw.UpIndent();

            foreach (var processorInfo in keypair.Value)
            {
               cw.WriteLine($"server.UseKeyHandler<{processorInfo.ClassInfo.FullTypeName}>();");
               token.ThrowIfCancellationRequested();
            }
         
            cw.WriteLine($"return server;");
            cw.DownIndent();
            cw.WriteLine($"}}");
         }
      }

      cw.WriteLine();
      
      cw.DownIndent();
      cw.WriteLine($"}}");
      
      token.ThrowIfCancellationRequested();
      context.AddSource($"RegistrationExtensions.g.cs", cw.ToString());
   }

   private static Dictionary<string, HashSet<ProcessorInfo>> GroupByRegistrationGroups(
      ImmutableArray<ProcessorInfo> processorInfos)
   {
      Dictionary<string, HashSet<ProcessorInfo>> groups = [];
      
      foreach (var processorInfo in processorInfos)
      {
         foreach (var group in processorInfo.RegistrationGroups)
         {
            if (processorInfo.IncludeServer)
            {
               GetGroupList($"Server{group}", groups).Add(processorInfo);
               GetGroupList($"Server", groups).Add(processorInfo);
            }

            if (!processorInfo.IncludeClient) continue;
            
            GetGroupList($"Client{group}", groups).Add(processorInfo);
         }

         if (processorInfo.IncludeServer)
         {
            GetGroupList($"Server", groups).Add(processorInfo);
         }
         
         if (processorInfo.IncludeClient)
         {
            GetGroupList($"Client", groups).Add(processorInfo);
         }
      }

      return groups;
   }

   private static HashSet<ProcessorInfo> GetGroupList(string name, Dictionary<string, HashSet<ProcessorInfo>> groups)
   {
      if (!groups.TryGetValue(name, out var serverList))
      {
         serverList = groups[name] = [];
      }
      
      return serverList;
   }
}