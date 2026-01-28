namespace CAP.BSP.DSP.Domain.Exceptions;

/// <summary>
/// Exception thrown when a claim occurrence date is in the future.
/// Business rule (US3): Claims can only be declared for past events.
/// </summary>
public class DateSurvenanceFutureException : Exception
{
    /// <summary>
    /// Initializes a new instance of the DateSurvenanceFutureException.
    /// </summary>
    public DateSurvenanceFutureException()
        : base("DateSurvenance invalide: la date de survenance ne peut pas être dans le futur")
    {
    }

    /// <summary>
    /// Initializes a new instance with a specific date.
    /// </summary>
    /// <param name="dateSurvenance">The invalid future date.</param>
    public DateSurvenanceFutureException(DateTime dateSurvenance)
        : base($"DateSurvenance invalide: la date de survenance ({dateSurvenance:yyyy-MM-dd}) ne peut pas être dans le futur")
    {
    }

    /// <summary>
    /// Initializes a new instance with a custom message.
    /// </summary>
    /// <param name="message">The error message.</param>
    public DateSurvenanceFutureException(string message)
        : base(message)
    {
    }

    /// <summary>
    /// Initializes a new instance with a custom message and inner exception.
    /// </summary>
    /// <param name="message">The error message.</param>
    /// <param name="innerException">The inner exception.</param>
    public DateSurvenanceFutureException(string message, Exception innerException)
        : base(message, innerException)
    {
    }
}
