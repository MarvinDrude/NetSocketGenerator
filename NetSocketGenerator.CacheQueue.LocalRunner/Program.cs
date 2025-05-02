using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NetSocketGenerator.CacheQueue.Client;
using NetSocketGenerator.CacheQueue.Client.Extensions;
using NetSocketGenerator.CacheQueue.Configuration.Server;
using NetSocketGenerator.CacheQueue.Extensions;
using NetSocketGenerator.CacheQueue.Server;
using Serilog;
using Serilog.Sinks.SystemConsole.Themes;

const string template = "[{Timestamp:HH:mm:ss} {Level:u3}]{Scope:lj} {Message:lj}{NewLine}{Exception}";
var log = new LoggerConfiguration()
   .MinimumLevel.Debug()
   .Enrich.FromLogContext()
   .WriteTo.Console(theme: AnsiConsoleTheme.Code, outputTemplate: template)
   .CreateLogger();

Log.Logger = log;

ServiceCollection collection = new();
collection.AddLogging(lb =>
{
   lb.ClearProviders();
   lb.AddSerilog(log);
});
collection.AddCacheQueue();
collection.AddCacheQueueClient();

var serviceProvider = collection.BuildServiceProvider();

var localCacheQueue = new CacheQueueServer(
   serviceProvider.GetRequiredService<ILogger<CacheQueueServer>>(),
   new CacheQueueServerOptions()
   {
      ServiceProvider = serviceProvider,
      Address = "127.0.0.1",
      Port = 34444,
   });
localCacheQueue.Start();

var testClient = new CacheQueueClient(new CacheQueueClientOptions()
{
   Address = "127.0.0.1",
   Port = 34444,
   ServiceProvider = serviceProvider,
   ServerAckTimeout = TimeSpan.FromSeconds(10)
});
testClient.Connect();

await Task.Delay(1000);
//testClient.QueueCreateNoAck("Messages");

const string queueName = "Messages";

await testClient.Queue.Create(queueName);
await testClient.Queue.Subscribe(queueName);

testClient.Queue.AddHandler<TestMessage>(queueName, async (context) =>
{
   Console.WriteLine(context.Message.Message);
   await context.Respond(new TestMessageBad() { Message = "comeback" });
});

// await testClient.Queue.Publish(queueName, new TestMessage()
// {
//    Message = "null"
// });
// testClient.Queue.PublishNoAck(queueName, new TestMessage()
// {
//    Message = "null1"
// });

Task.Factory.StartNew(async () =>
{
   var a = await testClient.Queue.PublishAndReceive<TestMessage, TestMessageBad>(queueName, new TestMessage()
   {
      Message = "null"
   });
   Console.WriteLine(a?.Message);
}, TaskCreationOptions.LongRunning);




Console.WriteLine("Finished");

while (true)
{
   await Task.Delay(60_000);
}

public sealed class TestMessage
{
   public required string Message { get; set; } 
}

public sealed class TestMessageBad
{
   public string? Message { get; set; } 
}