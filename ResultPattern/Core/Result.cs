namespace ResultPattern.Core;

/// <summary>
/// Represents the result of an operation that doesn't return a value.
/// </summary>
public class Result
{
    /// <summary>
    /// Indicates whether the operation was successful.
    /// </summary>
    public bool IsSuccess { get; }

    /// <summary>
    /// Indicates whether the operation failed.
    /// </summary>
    public bool IsFailure => !IsSuccess;

    /// <summary>
    /// The error if the operation failed; Error.None if successful.
    /// </summary>
    public Error Error { get; }

    protected Result(bool isSuccess, Error error)
    {
        if (isSuccess && error != Error.None)
            throw new ArgumentException("Success result cannot have an error", nameof(error));
        if (!isSuccess && error == Error.None)
            throw new ArgumentException("Failure result must have an error", nameof(error));

        IsSuccess = isSuccess;
        Error = error;
    }

    /// <summary>
    /// Creates a successful result.
    /// </summary>
    public static Result Success() => new(true, Error.None);

    /// <summary>
    /// Creates a failed result with the specified error.
    /// </summary>
    public static Result Failure(Error error) => new(false, error);

    /// <summary>
    /// Creates a successful result with a value.
    /// </summary>
    public static Result<TValue> Success<TValue>(TValue value) => Result<TValue>.Success(value);

    /// <summary>
    /// Creates a failed result with a value type.
    /// </summary>
    public static Result<TValue> Failure<TValue>(Error error) => Result<TValue>.Failure(error);

    /// <summary>
    /// Implicit conversion from Error to failed Result.
    /// </summary>
    public static implicit operator Result(Error error) => Failure(error);
}

/// <summary>
/// Represents the result of an operation that returns a value of type TValue.
/// </summary>
public class Result<TValue> : Result
{
    private readonly TValue? _value;

    /// <summary>
    /// The value if the operation was successful.
    /// Throws if accessed on a failed result.
    /// </summary>
    public TValue Value => IsSuccess
        ? _value!
        : throw new InvalidOperationException(
            $"Cannot access value of a failed result. Error: {Error.Code} - {Error.Description}");

    private Result(TValue value) : base(true, Error.None)
    {
        _value = value;
    }

    private Result(Error error) : base(false, error)
    {
        _value = default;
    }

    /// <summary>
    /// Creates a successful result with the specified value.
    /// </summary>
    public static Result<TValue> Success(TValue value) => new(value);

    /// <summary>
    /// Creates a failed result with the specified error.
    /// </summary>
    public new static Result<TValue> Failure(Error error) => new(error);

    /// <summary>
    /// Implicit conversion from TValue to successful Result.
    /// </summary>
    public static implicit operator Result<TValue>(TValue value) =>
        value is not null ? Success(value) : Failure(Error.Failure("Null", "Value cannot be null"));

    /// <summary>
    /// Implicit conversion from Error to failed Result.
    /// </summary>
    public static implicit operator Result<TValue>(Error error) => Failure(error);
}