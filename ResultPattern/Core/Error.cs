namespace ResultPattern.Core;

/// <summary>
/// Represents an error with a code and description.
/// </summary>
public record Error(string Code, string Description)
{
    /// <summary>
    /// Represents no error (used for successful results).
    /// </summary>
    public static readonly Error None = new(string.Empty, string.Empty);

    /// <summary>
    /// Creates a not found error.
    /// </summary>
    public static Error NotFound(string code, string description) =>
        new NotFoundError(code, description);

    /// <summary>
    /// Creates a validation error.
    /// </summary>
    public static Error Validation(string code, string description) =>
        new ValidationError(code, description);

    /// <summary>
    /// Creates a conflict error.
    /// </summary>
    public static Error Conflict(string code, string description) =>
        new ConflictError(code, description);

    /// <summary>
    /// Creates an unauthorized error.
    /// </summary>
    public static Error Unauthorized(string code, string description) =>
        new UnauthorizedError(code, description);

    /// <summary>
    /// Creates a forbidden error.
    /// </summary>
    public static Error Forbidden(string code, string description) =>
        new ForbiddenError(code, description);

    /// <summary>
    /// Creates a general failure error.
    /// </summary>
    public static Error Failure(string code, string description) =>
        new(code, description);
}

/// <summary>
/// Represents a "not found" error (404).
/// </summary>
public sealed record NotFoundError(string Code, string Description)
    : Error(Code, Description);

/// <summary>
/// Represents a validation error (400).
/// </summary>
public sealed record ValidationError(string Code, string Description)
    : Error(Code, Description);

/// <summary>
/// Represents a conflict error (409).
/// </summary>
public sealed record ConflictError(string Code, string Description)
    : Error(Code, Description);

/// <summary>
/// Represents an unauthorized error (401).
/// </summary>
public sealed record UnauthorizedError(string Code, string Description)
    : Error(Code, Description);

/// <summary>
/// Represents a forbidden error (403).
/// </summary>
public sealed record ForbiddenError(string Code, string Description)
    : Error(Code, Description);