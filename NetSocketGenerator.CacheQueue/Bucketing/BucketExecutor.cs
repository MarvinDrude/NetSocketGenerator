
namespace NetSocketGenerator.CacheQueue.Bucketing;

public sealed partial class BucketExecutor : IDisposable
{
   internal CacheQueueServer Server { get; }
   internal int Index { get; }
   
   private readonly ILogger<BucketExecutor> _logger;
   private readonly CancellationTokenSource _cts = new();
   
   private readonly Channel<BucketCommand> _commandChannel = Channel.CreateUnbounded<BucketCommand>();
   private readonly StringStore _stringStore;

   public BucketExecutor(
      ILogger<BucketExecutor> logger,
      CacheQueueServer server,
      int index)
   {
      _logger = logger;
      Server = server;
      Index = index;

      _stringStore = new StringStore(this);
      
      _ = Task.Factory.StartNew(
         RunCommands, 
         _cts.Token, 
         TaskCreationOptions.LongRunning, 
         TaskScheduler.Default);
   }
   
   public void Enqueue(BucketCommand command)
   {
      _commandChannel.Writer.TryWrite(command);
   }

   private async Task RunCommands()
   {
      using var scopeServer = _logger.BeginScope($"{Server.NodeName}");
      using var scope = _logger.BeginScope($"BucketExecutor_{Index}");
      
      while (!_cts.IsCancellationRequested
         && await _commandChannel.Reader.WaitToReadAsync(_cts.Token))
      {
         while (_commandChannel.Reader.TryRead(out var command))
         {
            try
            {
               Handle(command);
            }
            catch (Exception error)
            {
               LogFatalCommandError(error);
            }
         }
      }
   }
   
   private void Handle(BucketCommand command)
   {
      LogRunningCommand(command.SourceCommand);
      
      _ = command.SourceCommand.StoreType switch
      {
         StoreTypes.String => _stringStore.Handle(command),
         _ => false
      };
   }

   public void Dispose()
   {
      _cts.Cancel();
      _commandChannel.Writer.TryComplete();
      _cts.Dispose();
   }
}