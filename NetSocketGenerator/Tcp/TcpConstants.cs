namespace NetSocketGenerator.Tcp;

/// <summary>
/// Provides constant values related to TCP socket communication in the NetSocketGenerator.Tcp namespace.
/// </summary>
public static class TcpConstants
{
   /// <summary>
   /// Represents the buffer size, in bytes, used for safe operations
   /// on the stack to ensure reliable TCP socket communication.
   /// </summary>
   public const int SafeStackBufferSize = 1024;

   /// <summary>
   /// Maximum number of characters allowed in a range token type.
   /// </summary>
   public const int TokenCharRangeMaxCount = 256;
}