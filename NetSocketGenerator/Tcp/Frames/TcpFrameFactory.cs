namespace NetSocketGenerator.Tcp.Frames;

/// <inheritdoc cref="ITcpFrameFactory"/>
public sealed class TcpFrameFactory : ITcpFrameFactory
{
   public ITcpFrame Create()
   {
      return new TcpFrame();
   }
}