// Outcome of an application operation: either a value or an AppError.
namespace Auction.Core.Common;

public readonly record struct Unit
{
    public static readonly Unit Value = new();
}

public sealed class Result<T>
{
    private readonly T? value;

    private Result(T value, AppError? error)
    {
        this.value = value;
        Error = error;
    }

    public AppError? Error { get; }

    public bool IsSuccess => Error is null;

    public T Value => IsSuccess
        ? value!
        : throw new InvalidOperationException($"Result contains error '{Error!.Code}' and has no value.");

    public static Result<T> Success(T value) => new(value, null);

    public static Result<T> Failure(AppError error) => new(default!, error);

    public static implicit operator Result<T>(T value) => Success(value);

    public static implicit operator Result<T>(AppError error) => Failure(error);

    public TOut Match<TOut>(Func<T, TOut> onSuccess, Func<AppError, TOut> onFailure) =>
        IsSuccess ? onSuccess(value!) : onFailure(Error!);
}

public static class Result
{
    public static Result<Unit> Success() => Unit.Value;
}
