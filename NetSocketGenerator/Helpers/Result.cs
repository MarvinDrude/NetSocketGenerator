namespace NetSocketGenerator.Helpers;

public sealed class Result<TSuccess, TFailure>
{
   [MemberNotNullWhen(true, nameof(Value))]
   [MemberNotNullWhen(false, nameof(Error))]
   public bool IsSuccess { get; }

   public TFailure? Error { get; }
   public TSuccess? Value { get; }

   public Result(TSuccess success)
   {
      IsSuccess = true;
      Value = success;
   }

   public Result(TFailure failure)
   {
      IsSuccess = false;
      Error = failure;
   }
   
   public static implicit operator Result<TSuccess, TFailure>(TSuccess success)
      => new (success);

   public static implicit operator Result<TSuccess, TFailure>(TFailure failure)
      => new (failure);
}