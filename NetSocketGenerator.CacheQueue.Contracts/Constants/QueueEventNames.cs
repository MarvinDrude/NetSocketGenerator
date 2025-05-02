namespace NetSocketGenerator.CacheQueue.Contracts.Constants;

public static class QueueEventNames
{
   public const string Subscribe = "QUEUE_SUBSCRIBE";
   public const string Unsubscribe = "QUEUE_UNSUBSCRIBE";
   
   public const string Create = "QUEUE_CREATE";
   public const string Delete = "QUEUE_DELETE";

   public const string Publish = "QUEUE_PUBLISH";
   public const string PublishAck = "QUEUE_PUBLISH_ACK";
}