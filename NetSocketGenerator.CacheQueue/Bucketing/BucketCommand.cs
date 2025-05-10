
namespace NetSocketGenerator.CacheQueue.Bucketing;

public sealed class BucketCommand
{
   public required BaseCommand SourceCommand { get; init; }
}