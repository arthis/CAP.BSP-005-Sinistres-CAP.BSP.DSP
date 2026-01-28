namespace CAP.BSP.DSP.Domain.ValueObjects;

/// <summary>
/// Enumeration representing the status of a claim declaration.
/// </summary>
public enum StatutDeclaration
{
    /// <summary>
    /// Claim has been declared and is awaiting processing.
    /// </summary>
    Declaree,

    /// <summary>
    /// Claim has been validated and accepted.
    /// </summary>
    Validee,

    /// <summary>
    /// Claim has been cancelled or rejected.
    /// </summary>
    Annulee
}
