
namespace NetSocketGenerator.CacheQueue.Bucketing;

public sealed class BucketCommand
{
   public IAckSource? AckSource { get; init; }
   
   public required BaseCommand SourceCommand { get; init; }
}