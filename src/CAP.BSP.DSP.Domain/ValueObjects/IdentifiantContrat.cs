namespace CAP.BSP.DSP.Domain.ValueObjects;

/// <summary>
/// Value object representing a contract identifier.
/// Format: POL-YYYYMMDD-XXXXX (e.g., POL-20260127-00001)
/// </summary>
public record IdentifiantContrat
{
    private const string Pattern = @"^POL-\d{8}-\d{5}$";

    /// <summary>
    /// The contract identifier value.
    /// </summary>
    public string Value { get; }

    private IdentifiantContrat(string value)
    {
        Value = value;
    }

    /// <summary>
    /// Creates an IdentifiantContrat from a string value.
    /// </summary>
    /// <param name="value">The contract identifier string.</param>
    /// <returns>An IdentifiantContrat instance.</returns>
    /// <exception cref="ArgumentException">Thrown when the value is null, empty, or doesn't match the expected format.</exception>
    public static IdentifiantContrat Create(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new ArgumentException("IdentifiantContrat ne peut pas être vide", nameof(value));
        }

        if (!System.Text.RegularExpressions.Regex.IsMatch(value, Pattern))
        {
            throw new ArgumentException(
                $"IdentifiantContrat invalide: '{value}' ne correspond pas au format attendu (POL-YYYYMMDD-XXXXX)",
                nameof(value));
        }

        return new IdentifiantContrat(value);
    }

    /// <summary>
    /// Implicit conversion from IdentifiantContrat to string.
    /// </summary>
    public static implicit operator string(IdentifiantContrat identifiantContrat) => identifiantContrat.Value;

    /// <summary>
    /// String representation of the contract identifier.
    /// </summary>
    public override string ToString() => Value;
}
