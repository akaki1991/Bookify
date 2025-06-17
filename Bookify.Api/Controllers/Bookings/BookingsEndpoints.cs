using Bookify.Application.Bookings.Commands;
using Bookify.Application.Bookings.Queries;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Bookify.Api.Controllers.Bookings;

public static class BookingsEndpoints
{
    public static IEndpointRouteBuilder MapBookingEndpoints(this IEndpointRouteBuilder builder)
    {
        builder.MapGet("bookings/{id}", GetBooking)
               .WithName(nameof(GetBooking));

        builder.MapPost("bookings", ReserveBooking)
               .RequireAuthorization();

        return builder;
    }

    public static async Task<IResult> GetBooking(
        [FromRoute] Guid id,
        ISender sender,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(new GetBookingQuery(id), cancellationToken);
        return result.IsSuccess ? Results.Ok(result.Value) : Results.NotFound();
    }

    public static async Task<IResult> ReserveBooking(
        [FromBody] ReserveBookingCommand command,
        ISender sender,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(command, cancellationToken);

        if (result.IsFailure)
        {
            return Results.BadRequest(result.Error);
        }

        return Results.CreatedAtRoute(
            nameof(GetBooking),
            new { id = result.Value },
            result.Value);
    }
}
