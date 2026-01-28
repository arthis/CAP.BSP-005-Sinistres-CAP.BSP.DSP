namespace CAP.BSP.DSP.Domain.Exceptions;

/// <summary>
/// Exception thrown when a contract identifier is missing from a claim declaration.
/// Business rule: Every claim declaration MUST reference a valid contract.
/// </summary>
public class IdentifiantContratManquantException : Exception
{
    /// <summary>
    /// Initializes a new instance of the IdentifiantContratManquantException.
    /// </summary>
    public IdentifiantContratManquantException()
        : base("IdentifiantContrat obligatoire: toute déclaration de sinistre doit référencer un contrat valide")
    {
    }

    /// <summary>
    /// Initializes a new instance with a custom message.
    /// </summary>
    /// <param name="message">The error message.</param>
    public IdentifiantContratManquantException(string message)
        : base(message)
    {
    }

    /// <summary>
    /// Initializes a new instance with a custom message and inner exception.
    /// </summary>
    /// <param name="message">The error message.</param>
    /// <param name="innerException">The inner exception.</param>
    public IdentifiantContratManquantException(string message, Exception innerException)
        : base(message, innerException)
    {
    }
}
