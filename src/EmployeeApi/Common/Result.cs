namespace EmployeeApi.Common;

/// <summary>
/// Outcome of an operation that can fail for an expected business reason
/// (unknown id, duplicate email). Expected failures are returned, not thrown;
/// exceptions are reserved for genuine bugs.
/// </summary>
public sealed class Result
{
    private static readonly Result Succeeded = new(null);

    private Result(Error? error) => Error = error;

    public Error? Error { get; }

    public bool IsSuccess => Error is null;

    public static Result Success() => Succeeded;

    public static Result Failure(Error error)
    {
        ArgumentNullException.ThrowIfNull(error);
        return new Result(error);
    }

    public static implicit operator Result(Error error) => Failure(error);

    public TOut Match<TOut>(Func<TOut> onSuccess, Func<Error, TOut> onFailure) =>
        Error is null ? onSuccess() : onFailure(Error);
}

/// <summary>A <see cref="Result"/> that carries a value on success.</summary>
public sealed class Result<T>
{
    private Result(T? value, Error? error)
    {
        Value = value;
        Error = error;
    }

    /// <summary>The value on success; default on failure.</summary>
    public T? Value { get; }

    public Error? Error { get; }

    public bool IsSuccess => Error is null;

    public static Result<T> Success(T value) => new(value, null);

    public static Result<T> Failure(Error error)
    {
        ArgumentNullException.ThrowIfNull(error);
        return new Result<T>(default, error);
    }

    public static implicit operator Result<T>(T value) => Success(value);

    public static implicit operator Result<T>(Error error) => Failure(error);

    public TOut Match<TOut>(Func<T, TOut> onSuccess, Func<Error, TOut> onFailure) =>
        Error is null ? onSuccess(Value!) : onFailure(Error);
}
