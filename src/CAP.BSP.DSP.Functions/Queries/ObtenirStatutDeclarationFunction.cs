using System.Net;
using CAP.BSP.DSP.Application.Queries.ObtenirStatutDeclaration;
using MediatR;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;

namespace CAP.BSP.DSP.Functions.Queries;

public class ObtenirStatutDeclarationFunction
{
    private readonly IMediator _mediator;
    private readonly ILogger<ObtenirStatutDeclarationFunction> _logger;

    public ObtenirStatutDeclarationFunction(
        IMediator mediator,
        ILogger<ObtenirStatutDeclarationFunction> logger)
    {
        _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    [Function("ObtenirStatutDeclaration")]
    public async Task<HttpResponseData> Run(
        [HttpTrigger(AuthorizationLevel.Function, "get", Route = "v1/declarations/{identifiantSinistre}")] HttpRequestData req,
        string identifiantSinistre,
        CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation(
                "Received ObtenirStatutDeclaration request for claim {IdentifiantSinistre}",
                identifiantSinistre);

            if (string.IsNullOrWhiteSpace(identifiantSinistre))
            {
                var badRequestResponse = req.CreateResponse(HttpStatusCode.BadRequest);
                await badRequestResponse.WriteAsJsonAsync(new { Error = "IdentifiantSinistre is required" }, cancellationToken);
                return badRequestResponse;
            }

            // Create query
            var query = new ObtenirStatutDeclarationQuery
            {
                IdentifiantSinistre = identifiantSinistre
            };

            // Send query via MediatR
            var result = await _mediator.Send(query, cancellationToken);

            if (!result.IsSuccess)
            {
                // Check if it's a not found case
                if (result.ErrorMessage?.Contains("not found") == true)
                {
                    var notFoundResponse = req.CreateResponse(HttpStatusCode.NotFound);
                    await notFoundResponse.WriteAsJsonAsync(new { Error = result.ErrorMessage }, cancellationToken);
                    return notFoundResponse;
                }

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
            _logger.LogError(ex, "Unexpected error processing ObtenirStatutDeclaration request for claim {IdentifiantSinistre}", identifiantSinistre);
            var errorResponse = req.CreateResponse(HttpStatusCode.InternalServerError);
            await errorResponse.WriteAsJsonAsync(new { Error = "An unexpected error occurred" }, cancellationToken);
            return errorResponse;
        }
    }
}
