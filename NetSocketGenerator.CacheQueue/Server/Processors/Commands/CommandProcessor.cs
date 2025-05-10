
namespace NetSocketGenerator.CacheQueue.Server.Processors.Commands;

[SocketProcessor(
   EventNamePattern = EventNames.Command,
   RegistrationGroups = ["Commands"],
   IncludeClient = false
)]
public sealed partial class CommandProcessor
{
   private readonly ILogger<CommandProcessor> _logger;

   public CommandProcessor(
      ILogger<CommandProcessor> logger)
   {
      _logger = logger;
   }
   
   public async Task Execute(
      ITcpServerConnection connection,
      [SocketPayload] BaseCommand command)
   {
      var queueServer = connection.CurrentServer.GetMetadata<CacheQueueServer>();
      using var scope = _logger.BeginScope(queueServer.NodeName);
      
      if (!queueServer.Options.IsClustered && queueServer.BucketSelector is not null)
      {
         var bucket = queueServer.BucketSelector.ChooseBucketExecutor(command.KeyName);
         var task = queueServer.AckContainer.Enqueue<AckMessageBase>(
            command.RequestId, TimeSpan.FromSeconds(10));
         
         var bucketCommand = new BucketCommand()
         {
            SourceCommand = command,
         };
         
         bucket.Enqueue(bucketCommand);
         var result = await task;

         if (result is not null)
         {
            connection.Send(EventNames.CommandAck, result);
         }
      }
   }
}