using System.Net;
using CAP.BSP.DSP.Application.Queries.RechercherDeclarations;
using MediatR;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;

namespace CAP.BSP.DSP.Functions.Queries;

public class RechercherDeclarationsFunction
{
    private readonly IMediator _mediator;
    private readonly ILogger<RechercherDeclarationsFunction> _logger;

    public RechercherDeclarationsFunction(
        IMediator mediator,
        ILogger<RechercherDeclarationsFunction> logger)
    {
        _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    [Function("RechercherDeclarations")]
    public async Task<HttpResponseData> Run(
        [HttpTrigger(AuthorizationLevel.Function, "get", Route = "v1/declarations")] HttpRequestData req,
        CancellationToken cancellationToken)
    {
        try
        {
            // Parse query parameters
            var queryParameters = System.Web.HttpUtility.ParseQueryString(req.Url.Query);
            
            var identifiantContrat = queryParameters["identifiantContrat"];
            var statut = queryParameters["statut"];
            var limit = int.TryParse(queryParameters["limit"], out var parsedLimit) ? parsedLimit : 50;
            var offset = int.TryParse(queryParameters["offset"], out var parsedOffset) ? parsedOffset : 0;

            _logger.LogInformation(
                "Received RechercherDeclarations request with filters: identifiantContrat={IdentifiantContrat}, statut={Statut}, limit={Limit}, offset={Offset}",
                identifiantContrat ?? "null",
                statut ?? "null",
                limit,
                offset);

            // Create query
            var query = new RechercherDeclarationsQuery
            {
                IdentifiantContrat = identifiantContrat,
                Statut = statut,
                Limit = limit,
                Offset = offset
            };

            // Send query via MediatR
            var result = await _mediator.Send(query, cancellationToken);

            if (!result.IsSuccess)
            {
                var errorResponse = req.CreateResponse(HttpStatusCode.InternalServerError);
                await errorResponse.WriteAsJsonAsync(new { Error = result.ErrorMessage }, cancellationToken);
                return errorResponse;
            }

            // Create success response with 200 OK
            var response = req.CreateResponse(HttpStatusCode.OK);
            await response.WriteAsJsonAsync(result.Data, cancellationToken);
            
            return response;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error processing RechercherDeclarations request");
            var errorResponse = req.CreateResponse(HttpStatusCode.InternalServerError);
            await errorResponse.WriteAsJsonAsync(new { Error = "An unexpected error occurred" }, cancellationToken);
            return errorResponse;
        }
    }
}
