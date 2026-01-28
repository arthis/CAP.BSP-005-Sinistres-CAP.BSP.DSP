namespace CAP.BSP.DSP.Domain.ValueObjects;

/// <summary>
/// Value object representing a claim identifier.
/// Auto-generated in format: SIN-YYYY-NNNNNN (e.g., SIN-2026-000001)
/// </summary>
public record IdentifiantSinistre
{
    private const string Pattern = @"^SIN-\d{4}-\d{6}$";

    /// <summary>
    /// The claim identifier value.
    /// </summary>
    public string Value { get; }

    private IdentifiantSinistre(string value)
    {
        Value = value;
    }

    /// <summary>
    /// Creates an IdentifiantSinistre from a string value.
    /// </summary>
    /// <param name="value">The claim identifier string.</param>
    /// <returns>An IdentifiantSinistre instance.</returns>
    /// <exception cref="ArgumentException">Thrown when the value is null, empty, or doesn't match the expected format.</exception>
    public static IdentifiantSinistre Create(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new ArgumentException("IdentifiantSinistre ne peut pas être vide", nameof(value));
        }

        if (!System.Text.RegularExpressions.Regex.IsMatch(value, Pattern))
        {
            throw new ArgumentException(
                $"IdentifiantSinistre invalide: '{value}' ne correspond pas au format attendu (SIN-YYYY-NNNNNN)",
                nameof(value));
        }

        return new IdentifiantSinistre(value);
    }

    /// <summary>
    /// Implicit conversion from IdentifiantSinistre to string.
    /// </summary>
    public static implicit operator string(IdentifiantSinistre identifiantSinistre) => identifiantSinistre.Value;

    /// <summary>
    /// String representation of the claim identifier.
    /// </summary>
    public override string ToString() => Value;
}
