using ResultPattern.Core;

namespace ResultPattern.Extensions;

/// <summary>
/// Extension methods for functional operations on Result types.
/// </summary>
public static class ResultExtensions
{
    /// <summary>
    /// Transforms the value if the result is successful.
    /// </summary>
    public static Result<TOut> Map<TIn, TOut>(
        this Result<TIn> result,
        Func<TIn, TOut> mapper)
    {
        return result.IsSuccess
            ? Result<TOut>.Success(mapper(result.Value))
            : Result<TOut>.Failure(result.Error);
    }

    /// <summary>
    /// Async version of Map.
    /// </summary>
    public static async Task<Result<TOut>> MapAsync<TIn, TOut>(
        this Result<TIn> result,
        Func<TIn, Task<TOut>> mapper)
    {
        return result.IsSuccess
            ? Result<TOut>.Success(await mapper(result.Value))
            : Result<TOut>.Failure(result.Error);
    }

    /// <summary>
    /// Chains operations that return Results (flatMap/bind).
    /// </summary>
    public static Result<TOut> Bind<TIn, TOut>(
        this Result<TIn> result,
        Func<TIn, Result<TOut>> binder)
    {
        return result.IsSuccess
            ? binder(result.Value)
            : Result<TOut>.Failure(result.Error);
    }

    /// <summary>
    /// Async version of Bind.
    /// </summary>
    public static async Task<Result<TOut>> BindAsync<TIn, TOut>(
        this Result<TIn> result,
        Func<TIn, Task<Result<TOut>>> binder)
    {
        return result.IsSuccess
            ? await binder(result.Value)
            : Result<TOut>.Failure(result.Error);
    }

    /// <summary>
    /// Chains async Result with sync binder.
    /// </summary>
    public static async Task<Result<TOut>> Bind<TIn, TOut>(
        this Task<Result<TIn>> resultTask,
        Func<TIn, Result<TOut>> binder)
    {
        var result = await resultTask;
        return result.Bind(binder);
    }

    /// <summary>
    /// Chains async Result with async binder.
    /// </summary>
    public static async Task<Result<TOut>> BindAsync<TIn, TOut>(
        this Task<Result<TIn>> resultTask,
        Func<TIn, Task<Result<TOut>>> binder)
    {
        var result = await resultTask;
        return await result.BindAsync(binder);
    }

    /// <summary>
    /// Handles both success and failure cases, returning a common type.
    /// </summary>
    public static TOut Match<TIn, TOut>(
        this Result<TIn> result,
        Func<TIn, TOut> onSuccess,
        Func<Error, TOut> onFailure)
    {
        return result.IsSuccess
            ? onSuccess(result.Value)
            : onFailure(result.Error);
    }

    /// <summary>
    /// Handles both success and failure cases for Result without value.
    /// </summary>
    public static TOut Match<TOut>(
        this Result result,
        Func<TOut> onSuccess,
        Func<Error, TOut> onFailure)
    {
        return result.IsSuccess
            ? onSuccess()
            : onFailure(result.Error);
    }

    /// <summary>
    /// Async version of Match.
    /// </summary>
    public static async Task<TOut> MatchAsync<TIn, TOut>(
        this Task<Result<TIn>> resultTask,
        Func<TIn, TOut> onSuccess,
        Func<Error, TOut> onFailure)
    {
        var result = await resultTask;
        return result.Match(onSuccess, onFailure);
    }

    /// <summary>
    /// Executes an action if the result is successful, then returns the original result.
    /// </summary>
    public static Result<T> Tap<T>(
        this Result<T> result,
        Action<T> action)
    {
        if (result.IsSuccess)
            action(result.Value);
        return result;
    }

    /// <summary>
    /// Async version of Tap.
    /// </summary>
    public static async Task<Result<T>> TapAsync<T>(
        this Result<T> result,
        Func<T, Task> action)
    {
        if (result.IsSuccess)
            await action(result.Value);
        return result;
    }

    /// <summary>
    /// Executes an action if the result is a failure, then returns the original result.
    /// </summary>
    public static Result<T> TapError<T>(
        this Result<T> result,
        Action<Error> action)
    {
        if (result.IsFailure)
            action(result.Error);
        return result;
    }

    /// <summary>
    /// Converts a Result without value to a Result with a value.
    /// </summary>
    public static Result<T> ToResult<T>(this Result result, T value)
    {
        return result.IsSuccess
            ? Result<T>.Success(value)
            : Result<T>.Failure(result.Error);
    }

    /// <summary>
    /// Ensures a condition is met, or returns a failure.
    /// </summary>
    public static Result<T> Ensure<T>(
        this Result<T> result,
        Func<T, bool> predicate,
        Error error)
    {
        if (result.IsFailure)
            return result;

        return predicate(result.Value)
            ? result
            : Result<T>.Failure(error);
    }

    /// <summary>
    /// Combines multiple Results, returning the first failure or success with all values.
    /// </summary>
    public static Result<(T1, T2)> Combine<T1, T2>(
        Result<T1> result1,
        Result<T2> result2)
    {
        if (result1.IsFailure) return Result<(T1, T2)>.Failure(result1.Error);
        if (result2.IsFailure) return Result<(T1, T2)>.Failure(result2.Error);

        return Result<(T1, T2)>.Success((result1.Value, result2.Value));
    }

    /// <summary>
    /// Wraps a potentially throwing operation in a Result.
    /// </summary>
    public static Result<T> Try<T>(Func<T> operation, Func<Exception, Error>? errorHandler = null)
    {
        try
        {
            return Result<T>.Success(operation());
        }
        catch (Exception ex)
        {
            var error = errorHandler?.Invoke(ex) 
                ?? Error.Failure("Exception", ex.Message);
            return Result<T>.Failure(error);
        }
    }

    /// <summary>
    /// Async version of Try.
    /// </summary>
    public static async Task<Result<T>> TryAsync<T>(
        Func<Task<T>> operation,
        Func<Exception, Error>? errorHandler = null)
    {
        try
        {
            return Result<T>.Success(await operation());
        }
        catch (Exception ex)
        {
            var error = errorHandler?.Invoke(ex) 
                ?? Error.Failure("Exception", ex.Message);
            return Result<T>.Failure(error);
        }
    }
}