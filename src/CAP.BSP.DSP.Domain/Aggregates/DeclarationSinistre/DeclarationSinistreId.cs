namespace CAP.BSP.DSP.Domain.Aggregates.DeclarationSinistre;

/// <summary>
/// Value object representing the unique identifier for a DeclarationSinistre aggregate.
/// Uses GUID for globally unique identification.
/// </summary>
public record DeclarationSinistreId
{
    /// <summary>
    /// The GUID value.
    /// </summary>
    public Guid Value { get; }

    private DeclarationSinistreId(Guid value)
    {
        if (value == Guid.Empty)
        {
            throw new ArgumentException("DeclarationSinistreId ne peut pas être vide", nameof(value));
        }

        Value = value;
    }

    /// <summary>
    /// Creates a new DeclarationSinistreId with a new GUID.
    /// </summary>
    /// <returns>A new DeclarationSinistreId instance.</returns>
    public static DeclarationSinistreId New() => new(Guid.NewGuid());

    /// <summary>
    /// Creates a DeclarationSinistreId from an existing GUID.
    /// </summary>
    /// <param name="value">The GUID value.</param>
    /// <returns>A DeclarationSinistreId instance.</returns>
    public static DeclarationSinistreId Create(Guid value) => new(value);

    /// <summary>
    /// Implicit conversion from DeclarationSinistreId to Guid.
    /// </summary>
    public static implicit operator Guid(DeclarationSinistreId id) => id.Value;

    /// <summary>
    /// String representation of the ID.
    /// </summary>
    public override string ToString() => Value.ToString();
}
