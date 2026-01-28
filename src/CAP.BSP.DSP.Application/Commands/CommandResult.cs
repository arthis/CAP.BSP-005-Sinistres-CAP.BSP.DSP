namespace CAP.BSP.DSP.Application.Commands;

/// <summary>
/// Represents the result of a command execution using the Result pattern.
/// Encapsulates success/failure state and error details.
/// </summary>
public record CommandResult
{
    /// <summary>
    /// Indicates whether the command executed successfully.
    /// </summary>
    public bool IsSuccess { get; init; }

    /// <summary>
    /// Error message if the command failed (null if successful).
    /// </summary>
    public string? Error { get; init; }

    /// <summary>
    /// Error code for categorizing failures (e.g., "VALIDATION_ERROR", "BUSINESS_RULE_VIOLATION").
    /// </summary>
    public string? ErrorCode { get; init; }

    /// <summary>
    /// Optional data returned by the command (e.g., generated IDs, confirmation details).
    /// </summary>
    public object? Data { get; init; }

    /// <summary>
    /// Creates a successful command result.
    /// </summary>
    /// <param name="data">Optional data to return with the success result.</param>
    /// <returns>A successful CommandResult.</returns>
    public static CommandResult Success(object? data = null) => new()
    {
        IsSuccess = true,
        Data = data
    };

    /// <summary>
    /// Creates a failed command result.
    /// </summary>
    /// <param name="error">Error message describing the failure.</param>
    /// <param name="errorCode">Error code for categorization.</param>
    /// <returns>A failed CommandResult.</returns>
    public static CommandResult Failure(string error, string errorCode) => new()
    {
        IsSuccess = false,
        Error = error,
        ErrorCode = errorCode
    };
}
