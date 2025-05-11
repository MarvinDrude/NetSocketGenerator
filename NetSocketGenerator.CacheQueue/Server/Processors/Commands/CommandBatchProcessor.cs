
namespace NetSocketGenerator.CacheQueue.Server.Processors.Commands;

[SocketProcessor(
   EventNamePattern = EventNames.CommandBatch,
   RegistrationGroups = ["Commands"],
   IncludeClient = false
)]
public sealed partial class CommandBatchProcessor
{
   private readonly ILogger<CommandBatchProcessor> _logger;

   public CommandBatchProcessor(
      ILogger<CommandBatchProcessor> logger)
   {
      _logger = logger;
   }
   
   public async Task Execute(
      ITcpServerConnection connection,
      [SocketPayload] BatchCommand command)
   {
      var queueServer = connection.CurrentServer.GetMetadata<CacheQueueServer>();
      using var scope = _logger.BeginScope(queueServer.NodeName);
      
      if (!queueServer.Options.IsClustered 
          && queueServer.BucketSelector is not null
          && command.Commands.Count > 0)
      {
         List<Task<AckMessageBase?>> tasks = [.. command.Commands.Select(c => ExecuteCommand(queueServer, c))];
         await TaskUtils.WhenAllCatchAll(tasks);
         
         if (command.AwaitsAck)
         {
            List<AckMessageBase?> results = [];
            
            foreach (var task in tasks)
            {
               results.Add(await task);
            }
            
            connection.Send<AckMessageBase>(EventNames.CommandAck, new BatchCommandAck()
            {
               AckRequestId = command.RequestId,
               Acks = results
            });
         }
      }
   }

   private async Task<AckMessageBase?> ExecuteCommand(CacheQueueServer queueServer, BaseCommand command)
   {
      var bucket = queueServer.BucketSelector!.ChooseBucketExecutor(command.KeyName);
      var id = Guid.CreateVersion7();
      
      var task = queueServer.AckContainer.Enqueue<AckMessageBase>(
         id, TimeSpan.FromSeconds(10));
         
      var bucketCommand = new BucketCommand()
      {
         SourceCommand = command,
         Id = id
      };
      
      bucket.Enqueue(bucketCommand);
      var result = await task;

      return result;
   }
}