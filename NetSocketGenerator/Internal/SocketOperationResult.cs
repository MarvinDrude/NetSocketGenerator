
namespace NetSocketGenerator.Internal;

/// <summary>
/// Represents the result of a socket operation, encapsulating the details of
/// any error that occurred and the number of bytes transferred in the operation.
/// </summary>
internal readonly struct SocketOperationResult
{
   /// <summary>
   /// Represents the specific error, if any, that occurred during a socket operation.
   /// This value is null if the operation is completed successfully.
   /// When this property is not null, it indicates the details of the exception encountered.
   /// </summary>
   public readonly SocketException? SocketError;

   /// <summary>
   /// Represents the total number of bytes successfully transferred during the socket operation.
   /// A value of 0 indicates no data was transferred or an error occurred.
   /// </summary>
   public readonly int BytesTransferred;

   /// <summary>
   /// Indicates whether an error occurred during the socket operation.
   /// This property returns true if the operation encountered an error; otherwise, false.
   /// </summary>
   [MemberNotNullWhen(true, nameof(SocketError))]
   public readonly bool IsError => SocketError is not null;

   /// <summary>
   /// Instantiates as a successful result with bytes transferred
   /// </summary>
   public SocketOperationResult(int bytesTransferred)
   {
      SocketError = null;
      BytesTransferred = bytesTransferred;
   }
   
   /// <summary>
   /// Instantiates as an error result with the exception
   /// </summary>
   public SocketOperationResult(SocketException error)
   {
      SocketError = error;
      BytesTransferred = 0;
   }
}