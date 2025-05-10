namespace NetSocketGenerator.Acknowledge;

public interface IAckSource
{
   public bool SetResult(object boxed);
}