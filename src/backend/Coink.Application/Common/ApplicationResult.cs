namespace Coink.Application.Common;

/// <summary>
/// Classifies expected application failures for transport-independent mapping.
/// </summary>
public enum ApplicationErrorType
{
    /// <summary>One or more inputs failed validation.</summary>
    Validation,

    /// <summary>The requested resource does not exist.</summary>
    NotFound,

    /// <summary>The supplied geographic identifiers do not form one valid hierarchy.</summary>
    InvalidGeography,
}

/// <summary>
/// Describes an expected application failure without transport or database details.
/// </summary>
/// <param name="Type">Failure classification used by transport mapping.</param>
/// <param name="Code">Stable machine-readable error code.</param>
/// <param name="Message">Safe human-readable explanation.</param>
/// <param name="ValidationErrors">Field errors when <paramref name="Type"/> is validation.</param>
public sealed record ApplicationError(
    ApplicationErrorType Type,
    string Code,
    string Message,
    IReadOnlyDictionary<string, string[]>? ValidationErrors = null);

/// <summary>
/// Carries either a successful value or one expected application error.
/// </summary>
/// <typeparam name="T">Successful result type.</typeparam>
public sealed class ApplicationResult<T>
{
    internal ApplicationResult(T? value, ApplicationError? error)
    {
        Value = value;
        Error = error;
    }

    /// <summary>Gets whether the operation completed successfully.</summary>
    public bool IsSuccess => Error is null;

    /// <summary>Gets the successful value, or default when the operation failed.</summary>
    public T? Value { get; }

    /// <summary>Gets the expected error, or <see langword="null"/> on success.</summary>
    public ApplicationError? Error { get; }

}

/// <summary>
/// Creates typed application results without static members on generic types.
/// </summary>
public static class ApplicationResults
{
    /// <summary>Creates a successful result.</summary>
    /// <typeparam name="T">Successful result type.</typeparam>
    /// <param name="value">Operation value.</param>
    /// <returns>A successful result.</returns>
    public static ApplicationResult<T> Success<T>(T value)
    {
        ArgumentNullException.ThrowIfNull(value);
        return new ApplicationResult<T>(value, error: null);
    }

    /// <summary>Creates a failed result.</summary>
    /// <param name="error">Expected application error.</param>
    /// <returns>A failed result.</returns>
    /// <typeparam name="T">Successful result type the operation would have returned.</typeparam>
    public static ApplicationResult<T> Failure<T>(ApplicationError error)
    {
        ArgumentNullException.ThrowIfNull(error);
        return new ApplicationResult<T>(value: default, error);
    }
}
