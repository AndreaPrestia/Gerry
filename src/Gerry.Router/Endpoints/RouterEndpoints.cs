using Gerry.Core.Entities;
using Gerry.Router.Services;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;

namespace Gerry.Router.Endpoints;

internal static class RouterEndpoints
{
    public static void MapRouterEndpoints(this IEndpointRouteBuilder endpointRouteBuilder)
    {
        ArgumentNullException.ThrowIfNull(endpointRouteBuilder);

        endpointRouteBuilder.MapPost("/messages/{topic}/dispatch", async (CancellationToken cancellationToken, [FromServices] RouterService service,
                [FromRoute] string? topic, [FromBody] Message message) =>
            {
                var result = await service.DispatchAsync(new Topic(topic), message, cancellationToken);

                return !result ? Results.BadRequest("Failed operation") : Results.Created("/dispatch", message);
            }).WithName("MessageDispatch").Produces<CreatedResult>(StatusCodes.Status201Created)
            .Produces<BadRequestResult>(StatusCodes.Status400BadRequest)
            .Produces<UnauthorizedResult>(StatusCodes.Status401Unauthorized)
            .Produces<ForbidResult>(StatusCodes.Status403Forbidden);

        endpointRouteBuilder.MapPost("/messages/{id}/consume",
                 ([FromServices] RouterService service, [FromRoute] Guid id,
                    [FromBody] ConsumedMessage message) =>
                {
                    var result = service.Consume(id, message);

                    return !result ? Results.BadRequest("Failed operation") : Results.Created("/consume", message);
                }).WithName("MessageConsume").Produces<CreatedResult>(StatusCodes.Status201Created)
            .Produces<BadRequestResult>(StatusCodes.Status400BadRequest)
            .Produces<UnauthorizedResult>(StatusCodes.Status401Unauthorized)
            .Produces<ForbidResult>(StatusCodes.Status403Forbidden);

        endpointRouteBuilder.MapPost("/messages/{id}/error",
                 ([FromServices] RouterService service, [FromRoute] Guid id,
                    [FromBody] ErrorMessage message) =>
                {
                    var result = service.Error(id, message);

                    return !result ? Results.BadRequest("Failed operation") : Results.Created("/error", message);
                }).WithName("ErrorMessageAdd").Produces<CreatedResult>(StatusCodes.Status201Created)
            .Produces<BadRequestResult>(StatusCodes.Status400BadRequest)
            .Produces<UnauthorizedResult>(StatusCodes.Status401Unauthorized)
            .Produces<ForbidResult>(StatusCodes.Status403Forbidden);
    }
}