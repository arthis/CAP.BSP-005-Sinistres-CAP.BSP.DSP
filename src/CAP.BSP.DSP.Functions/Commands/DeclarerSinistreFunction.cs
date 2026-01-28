using System.Net;
using CAP.BSP.DSP.Application.Commands.DeclarerSinistre;
using MediatR;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace CAP.BSP.DSP.Functions.Commands;

public class DeclarerSinistreFunction
{
    private readonly IMediator _mediator;
    private readonly ILogger<DeclarerSinistreFunction> _logger;

    public DeclarerSinistreFunction(
        IMediator mediator,
        ILogger<DeclarerSinistreFunction> logger)
    {
        _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    [Function("DeclarerSinistre")]
    public async Task<HttpResponseData> Run(
        [HttpTrigger(AuthorizationLevel.Function, "post", Route = "v1/declarations")] HttpRequestData req,
        CancellationToken cancellationToken)
    {
        try
        {
            // Read and deserialize request body
            var requestBody = await new StreamReader(req.Body).ReadToEndAsync(cancellationToken);
            var request = JsonSerializer.Deserialize<DeclarerSinistreRequest>(requestBody, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            if (request == null)
            {
                var badRequestResponse = req.CreateResponse(HttpStatusCode.BadRequest);
                await badRequestResponse.WriteAsJsonAsync(new { Error = "Invalid request body" }, cancellationToken);
                return badRequestResponse;
            }

            // Generate correlation ID if not provided
            var correlationId = req.Headers.TryGetValues("X-Correlation-ID", out var correlationValues)
                ? correlationValues.FirstOrDefault() ?? Guid.NewGuid().ToString()
                : Guid.NewGuid().ToString();

            // Get user ID from headers or authentication context
            var userId = req.Headers.TryGetValues("X-User-ID", out var userValues)
                ? userValues.FirstOrDefault() ?? "anonymous"
                : "anonymous";

            // Create command
            var command = new DeclarerSinistreCommand
            {
                IdentifiantContrat = request.IdentifiantContrat,
                DateSurvenance = request.DateSurvenance,
                CorrelationId = correlationId,
                UserId = userId
            };

            // Send command via MediatR
            var result = await _mediator.Send(command, cancellationToken);

            if (!result.IsSuccess)
            {
                var errorResponse = req.CreateResponse(HttpStatusCode.BadRequest);
                await errorResponse.WriteAsJsonAsync(new { Error = result.Error }, cancellationToken);
                return errorResponse;
            }

            // Create success response with 201 Created
            var response = req.CreateResponse(HttpStatusCode.Created);
            
            // Set Location header with the new resource URI
            var resultData = JsonSerializer.Deserialize<DeclarerSinistreResultData>(
                JsonSerializer.Serialize(result.Data));
            
            if (resultData?.IdentifiantSinistre != null)
            {
                var baseUrl = $"{req.Url.Scheme}://{req.Url.Authority}";
                response.Headers.Add("Location", $"{baseUrl}/api/v1/declarations/{resultData.IdentifiantSinistre}");
            }

            await response.WriteAsJsonAsync(result.Data, cancellationToken);
            response.StatusCode = HttpStatusCode.Created; // Ensure status code is set after WriteAsJsonAsync
            
            return response;
        }
        catch (JsonException ex)
        {
            _logger.LogError(ex, "Failed to deserialize request body");
            var badRequestResponse = req.CreateResponse(HttpStatusCode.BadRequest);
            await badRequestResponse.WriteAsJsonAsync(new { Error = "Invalid JSON format" }, cancellationToken);
            return badRequestResponse;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error processing DeclarerSinistre request");
            var errorResponse = req.CreateResponse(HttpStatusCode.InternalServerError);
            await errorResponse.WriteAsJsonAsync(new { Error = "An unexpected error occurred" }, cancellationToken);
            return errorResponse;
        }
    }
}

public record DeclarerSinistreRequest
{
    public string IdentifiantContrat { get; init; } = string.Empty;
    public DateTime DateSurvenance { get; init; }
}

public record DeclarerSinistreResultData
{
    public string? IdentifiantSinistre { get; init; }
    public DateTime DateDeclaration { get; init; }
}
