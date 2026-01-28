namespace CAP.BSP.DSP.Domain.ValueObjects;

/// <summary>
/// Value object representing the date when a claim occurred.
/// Must be today or in the past (US3: cannot be in the future).
/// </summary>
public record DateSurvenance
{
    /// <summary>
    /// The date when the claim occurred.
    /// </summary>
    public DateTime Value { get; }

    private DateSurvenance(DateTime value)
    {
        Value = value.Date; // Normalize to date only (no time component)
    }

    /// <summary>
    /// Creates a DateSurvenance from a DateTime value.
    /// </summary>
    /// <param name="value">The occurrence date.</param>
    /// <returns>A DateSurvenance instance.</returns>
    /// <exception cref="ArgumentException">Thrown when the date is in the future.</exception>
    public static DateSurvenance Create(DateTime value)
    {
        var dateOnly = value.Date;
        var today = DateTime.UtcNow.Date;

        if (dateOnly > today)
        {
            throw new ArgumentException(
                $"DateSurvenance invalide: la date de survenance ({dateOnly:yyyy-MM-dd}) ne peut pas être dans le futur",
                nameof(value));
        }

        return new DateSurvenance(dateOnly);
    }

    /// <summary>
    /// Implicit conversion from DateSurvenance to DateTime.
    /// </summary>
    public static implicit operator DateTime(DateSurvenance dateSurvenance) => dateSurvenance.Value;

    /// <summary>
    /// String representation of the occurrence date.
    /// </summary>
    public override string ToString() => Value.ToString("yyyy-MM-dd");
}
