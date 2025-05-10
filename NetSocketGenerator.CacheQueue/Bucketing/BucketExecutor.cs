
namespace NetSocketGenerator.CacheQueue.Bucketing;

public sealed partial class BucketExecutor : IDisposable
{
   internal CacheQueueServer Server { get; }
   internal int Index { get; }
   
   private readonly ILogger<BucketExecutor> _logger;
   private readonly CancellationTokenSource _cts = new();
   
   private readonly Channel<BucketCommand> _commandChannel = Channel.CreateUnbounded<BucketCommand>();
   
   private readonly StringStore _stringStore;
   private readonly ULongStore _uLongStore;
   private readonly LongStore _longStore;
   private readonly IntegerStore _integerStore;
   private readonly DoubleStore _doubleStore;

   public BucketExecutor(
      ILogger<BucketExecutor> logger,
      CacheQueueServer server,
      int index)
   {
      _logger = logger;
      Server = server;
      Index = index;

      _stringStore = new StringStore(this);
      _longStore = new LongStore(this);
      _uLongStore = new ULongStore(this);
      _doubleStore = new DoubleStore(this);
      _integerStore = new IntegerStore(this);
      
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
         StoreTypes.Double => _doubleStore.Handle(command),
         StoreTypes.Integer => _integerStore.Handle(command),
         StoreTypes.Long => _longStore.Handle(command),
         StoreTypes.ULong => _uLongStore.Handle(command),
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