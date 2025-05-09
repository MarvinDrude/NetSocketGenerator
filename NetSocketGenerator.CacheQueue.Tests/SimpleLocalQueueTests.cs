using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NetSocketGenerator.CacheQueue.Client;
using NetSocketGenerator.CacheQueue.Client.Extensions;
using NetSocketGenerator.CacheQueue.Configuration.Server;
using NetSocketGenerator.CacheQueue.Extensions;
using NetSocketGenerator.CacheQueue.Server;
using Serilog;
using Serilog.Sinks.SystemConsole.Themes;
using TUnit.Core.Interfaces;

namespace NetSocketGenerator.CacheQueue.Tests;

public sealed class SimpleLocalQueueTests
{
   [Test, NotInParallel]
   [ClassDataSource<CacheQueueLocalServerFactory, CacheQueueLocalClientFactory>(Shared = [SharedType.None])]
   public async Task TestSimpleSendAndSubscribe(
      CacheQueueLocalServerFactory serverFactory,
      CacheQueueLocalClientFactory clientFactory)
   {
      const string queueName = "test";
      
      var server = serverFactory.Server;
      var client = clientFactory.Client;

      long count = 0;
      
      client.Queue.AddHandler<ExampleMessage>(queueName, (context) =>
      {
         Interlocked.Add(ref count, context.Message.Number);
         return Task.CompletedTask;
      });
      
      server.Start();
      client.Connect();

      await client.Queue.Create(queueName);
      await client.Queue.Subscribe(queueName);
      
      client.Queue.PublishNoAck(queueName, new ExampleMessage() { Number = 1 });
      client.Queue.PublishNoAck(queueName, new ExampleMessage() { Number = 10 });
      await client.Queue.Publish(queueName, new ExampleMessage() { Number = 20 });
      client.Queue.PublishNoAck(queueName, new ExampleMessage() { Number = 6 });
      await client.Queue.Publish(queueName, new ExampleMessage() { Number = 32 });

      await Assert.That(count).IsEqualTo(1 + 10 + 20 + 6 + 32);
   }
   
   [Test, NotInParallel]
   [ClassDataSource<CacheQueueLocalServerFactory, CacheQueueLocalClientFactory>(Shared = [SharedType.None])]
   public async Task TestSimpleSendAndSubscribeUselessRespond(
      CacheQueueLocalServerFactory serverFactory,
      CacheQueueLocalClientFactory clientFactory)
   {
      const string queueName = "test";
      
      var server = serverFactory.Server;
      var client = clientFactory.Client;

      long count = 0;
      
      client.Queue.AddHandler<ExampleMessage>(queueName, async (context) =>
      {
         Interlocked.Add(ref count, context.Message.Number);
         await context.Respond(new ExampleMessage() { Number = 1 });
      });
      
      server.Start();
      client.Connect();

      await client.Queue.Create(queueName);
      await client.Queue.Subscribe(queueName);
      
      client.Queue.PublishNoAck(queueName, new ExampleMessage() { Number = 1 });
      client.Queue.PublishNoAck(queueName, new ExampleMessage() { Number = 10 });
      await client.Queue.Publish(queueName, new ExampleMessage() { Number = 20 });
      client.Queue.PublishNoAck(queueName, new ExampleMessage() { Number = 6 });
      await client.Queue.Publish(queueName, new ExampleMessage() { Number = 32 });

      await Assert.That(count).IsEqualTo(1 + 10 + 20 + 6 + 32);
   }
   
   [Test, NotInParallel]
   [ClassDataSource<CacheQueueLocalServerFactory, CacheQueueLocalClientFactory>(Shared = [SharedType.None])]
   public async Task TestSimpleSendAndReceive(
      CacheQueueLocalServerFactory serverFactory,
      CacheQueueLocalClientFactory clientFactory)
   {
      const string queueName = "test";
      
      var server = serverFactory.Server;
      var client = clientFactory.Client;

      long count = 0;
      
      client.Queue.AddHandler<ExampleMessage>(queueName, (context) =>
      {
         Interlocked.Add(ref count, context.Message.Number);
         context.RespondNoAck(new ExampleMessage() { Message = $"{context.Message.Number}" });
         return Task.CompletedTask;
      });
      
      server.Start();
      client.Connect();

      await client.Queue.Create(queueName);
      await client.Queue.Subscribe(queueName);

      var resp = await client.Queue.PublishAndReceive<ExampleMessage, ExampleMessage>(queueName, new ExampleMessage()
      {
         Number = 20,
      });
      var resp1 = await client.Queue.PublishAndReceive<ExampleMessage, ExampleMessage>(queueName, new ExampleMessage()
      {
         Number = 30,
      });
      
      await Assert.That(count).IsEqualTo(20 + 30);
      await Assert.That(resp?.Message + resp1?.Message).IsEqualTo("2030");
   }

   public sealed class ExampleMessage
   {
      public int Number { get; set; }
      
      public string? Message { get; set; }
   }
}

public sealed class CacheQueueLocalServerFactory : IAsyncInitializer, IAsyncDisposable
{
   public CacheQueueServer Server { get; set; } = null!;
   
   public Task InitializeAsync()
   {
      const string template = "[{Timestamp:HH:mm:ss} {Level:u3}]{Scope:lj} {Message:lj}{NewLine}{Exception}";
      var log = new LoggerConfiguration()
         .MinimumLevel.Debug()
         .Enrich.FromLogContext()
         .WriteTo.Console(theme: AnsiConsoleTheme.Code, outputTemplate: template)
         .CreateLogger();

      Log.Logger = log;
      
      var collection = new ServiceCollection();
      collection.AddCacheQueue();
      collection.AddLogging(lb =>
      {
         lb.ClearProviders();
         lb.AddSerilog(log);
      });
      
      var serviceProvider = collection.BuildServiceProvider();
      
      Server = new CacheQueueServer(
         serviceProvider.GetRequiredService<ILogger<CacheQueueServer>>(),
         new CacheQueueServerOptions()
      {
         Address = "127.0.0.1",
         Port = 45343,
         ServiceProvider = serviceProvider,
      });
      
      return Task.CompletedTask;
   }

   public async ValueTask DisposeAsync()
   {
      await Server.Stop();
   }
}

public sealed class CacheQueueLocalClientFactory : IAsyncInitializer, IAsyncDisposable
{
   public CacheQueueClient Client { get; set; } = null!;
   
   public Task InitializeAsync()
   {
      var collection = new ServiceCollection();
      collection.AddCacheQueueClient();
      
      var serviceProvider = collection.BuildServiceProvider();
      
      Client = new CacheQueueClient(new CacheQueueClientOptions()
      {
         Address = "127.0.0.1",
         Port = 45343,
         ServiceProvider = serviceProvider,
         ServerAckTimeout = TimeSpan.FromSeconds(3),
         QueueConsumerAckTimeout = TimeSpan.FromSeconds(6)
      });
      
      return Task.CompletedTask;
   }

   public async ValueTask DisposeAsync()
   {
      await Client.Disconnect();
   }
}