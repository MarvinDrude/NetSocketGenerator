namespace NetSocketGenerator.CacheQueue.Client.Batches;

public class BatchBuilder
{
   public DoubleBatchModule Doubles { get; }
   public IntegerBatchModule Integers { get; }
   public LongBatchModule Longs { get; }
   public ULongBatchModule ULongs { get; }
   public StringBatchModule Strings { get; }
   
   internal readonly CacheQueueClient Client;
   
   public BatchBuilder(CacheQueueClient client)
   {
      Client = client;

      Doubles = new DoubleBatchModule(this);
      Integers = new IntegerBatchModule(this);
      Longs = new LongBatchModule(this);
      ULongs = new ULongBatchModule(this);
      Strings = new StringBatchModule(this);
   }
   
   private List<BaseCommand> Commands { get; } = [];

   public void AddCommand(BaseCommand command)
   {
      Commands.Add(command);
   }

   public async Task<BatchResult> Send()
   {
      if (Commands.Count == 0)
      {
         throw new InvalidOperationException("No commands to send.");
      }

      var command = new BatchCommand()
      {
         Commands = Commands,
         KeyName = string.Empty,
         AwaitsAck = true
      };
      
      var task = Client.AckContainer
         .Enqueue<BatchCommandAck>(command.RequestId, Client.Options.ServerAckTimeout);

      Client.Tcp.Send<BaseCommand>(EventNames.CommandBatch, command);
      var result = await task;

      return new BatchResult(result);
   }
}