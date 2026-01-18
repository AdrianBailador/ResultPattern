using Microsoft.AspNetCore.Mvc;
using ResultPattern.Core;

namespace ResultPattern.Extensions;

/// <summary>
/// Extension methods to convert Result types to ASP.NET Core IResult responses.
/// </summary>
public static class ResultApiExtensions
{
    /// <summary>
    /// Converts a Result<T> to an appropriate IResult response.
    /// </summary>
    public static IResult ToApiResult<T>(this Result<T> result)
    {
        if (result.IsSuccess)
            return Results.Ok(result.Value);

        return ToErrorResult(result.Error);
    }

    /// <summary>
    /// Converts a Result to an appropriate IResult response.
    /// </summary>
    public static IResult ToApiResult(this Result result, object? successValue = null)
    {
        if (result.IsSuccess)
            return successValue is not null
                ? Results.Ok(successValue)
                : Results.Ok();

        return ToErrorResult(result.Error);
    }

    /// <summary>
    /// Converts a Result<T> to a Created response on success.
    /// </summary>
    public static IResult ToCreatedResult<T>(this Result<T> result, string uri)
    {
        if (result.IsSuccess)
            return Results.Created(uri, result.Value);

        return ToErrorResult(result.Error);
    }

    /// <summary>
    /// Converts a Result<T> to a Created response with dynamic URI.
    /// </summary>
    public static IResult ToCreatedResult<T>(this Result<T> result, Func<T, string> uriFactory)
    {
        if (result.IsSuccess)
            return Results.Created(uriFactory(result.Value), result.Value);

        return ToErrorResult(result.Error);
    }

    /// <summary>
    /// Converts a Result to NoContent on success.
    /// </summary>
    public static IResult ToNoContentResult(this Result result)
    {
        if (result.IsSuccess)
            return Results.NoContent();

        return ToErrorResult(result.Error);
    }

    /// <summary>
    /// Converts an Error to the appropriate IResult based on error type.
    /// </summary>
    private static IResult ToErrorResult(Error error)
    {
        var problemDetails = CreateProblemDetails(error);

        return error switch
        {
            NotFoundError => Results.NotFound(problemDetails),
            ValidationError => Results.BadRequest(problemDetails),
            ConflictError => Results.Conflict(problemDetails),
            UnauthorizedError => Results.Json(problemDetails, statusCode: 401),
            ForbiddenError => Results.Json(problemDetails, statusCode: 403),
            _ => Results.BadRequest(problemDetails)
        };
    }

    /// <summary>
    /// Creates a ProblemDetails object from an Error.
    /// </summary>
    private static ProblemDetails CreateProblemDetails(Error error)
    {
        var statusCode = error switch
        {
            NotFoundError => StatusCodes.Status404NotFound,
            ValidationError => StatusCodes.Status400BadRequest,
            ConflictError => StatusCodes.Status409Conflict,
            UnauthorizedError => StatusCodes.Status401Unauthorized,
            ForbiddenError => StatusCodes.Status403Forbidden,
            _ => StatusCodes.Status400BadRequest
        };

        return new ProblemDetails
        {
            Status = statusCode,
            Title = GetTitleForError(error),
            Detail = error.Description,
            Type = $"https://httpstatuses.com/{statusCode}",
            Extensions =
            {
                ["errorCode"] = error.Code
            }
        };
    }

    /// <summary>
    /// Gets a human-readable title based on error type.
    /// </summary>
    private static string GetTitleForError(Error error)
    {
        return error switch
        {
            NotFoundError => "Resource Not Found",
            ValidationError => "Validation Error",
            ConflictError => "Conflict",
            UnauthorizedError => "Unauthorized",
            ForbiddenError => "Forbidden",
            _ => "Bad Request"
        };
    }
}