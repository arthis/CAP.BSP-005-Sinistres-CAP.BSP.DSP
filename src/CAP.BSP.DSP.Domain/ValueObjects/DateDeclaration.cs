namespace CAP.BSP.DSP.Domain.ValueObjects;

/// <summary>
/// Value object representing the date when a claim was declared.
/// System-generated timestamp (not provided by user).
/// </summary>
public record DateDeclaration
{
    /// <summary>
    /// The declaration timestamp (UTC).
    /// </summary>
    public DateTime Value { get; }

    private DateDeclaration(DateTime value)
    {
        Value = value;
    }

    /// <summary>
    /// Creates a DateDeclaration with the current UTC timestamp.
    /// </summary>
    /// <returns>A DateDeclaration instance with current time.</returns>
    public static DateDeclaration Now() => new(DateTime.UtcNow);

    /// <summary>
    /// Creates a DateDeclaration from a specific DateTime value.
    /// Used for rehydration from event store.
    /// </summary>
    /// <param name="value">The declaration timestamp.</param>
    /// <returns>A DateDeclaration instance.</returns>
    public static DateDeclaration Create(DateTime value) => new(value);

    /// <summary>
    /// Implicit conversion from DateDeclaration to DateTime.
    /// </summary>
    public static implicit operator DateTime(DateDeclaration dateDeclaration) => dateDeclaration.Value;

    /// <summary>
    /// String representation of the declaration timestamp.
    /// </summary>
    public override string ToString() => Value.ToString("O"); // ISO 8601 format
}
