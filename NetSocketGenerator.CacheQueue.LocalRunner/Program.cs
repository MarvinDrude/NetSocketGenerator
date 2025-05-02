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

var w = await testClient.QueueCreate("Messages");
_ = "";

while (true)
{
   await Task.Delay(60_000);
}