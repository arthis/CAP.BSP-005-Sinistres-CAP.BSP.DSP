using CAP.BSP.DSP.Application.Commands;
using MediatR;

namespace CAP.BSP.DSP.Application.Queries.RechercherDeclarations;

/// <summary>
/// Query to search for claim declarations with optional filters.
/// Returns a paginated list of declarations.
/// </summary>
public record RechercherDeclarationsQuery : IRequest<QueryResult<RechercherDeclarationsResult>>
{
    /// <summary>
    /// Filter by contract identifier (optional).
    /// </summary>
    public string? IdentifiantContrat { get; init; }

    /// <summary>
    /// Filter by claim status (optional): Declaree, Validee, Annulee.
    /// </summary>
    public string? Statut { get; init; }

    /// <summary>
    /// Maximum number of results to return (default: 50, max: 100).
    /// </summary>
    public int Limit { get; init; } = 50;

    /// <summary>
    /// Number of results to skip for pagination (default: 0).
    /// </summary>
    public int Offset { get; init; } = 0;
}

/// <summary>
/// Result wrapper for query responses.
/// </summary>
/// <typeparam name="T">Type of the result data.</typeparam>
public record QueryResult<T>
{
    public bool IsSuccess { get; init; }
    public T? Data { get; init; }
    public string? ErrorMessage { get; init; }

    public static QueryResult<T> Success(T data) => new()
    {
        IsSuccess = true,
        Data = data
    };

    public static QueryResult<T> Failure(string errorMessage) => new()
    {
        IsSuccess = false,
        ErrorMessage = errorMessage
    };
}

/// <summary>
/// Result data for RechercherDeclarations query.
/// </summary>
public record RechercherDeclarationsResult
{
    public List<DeclarationListItemDto> Declarations { get; init; } = new();
    public int TotalCount { get; init; }
    public int Limit { get; init; }
    public int Offset { get; init; }
}
