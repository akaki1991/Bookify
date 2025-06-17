using Bookify.Application.Abstractions.Caching;
using Bookify.Application.Abstractions.Data;
using Bookify.Application.Abstractions.Messaging;
using Bookify.Domain.Abstractions;
using Bookify.Domain.Bookings;
using Dapper;

namespace Bookify.Application.Bookings.Queries;

internal sealed class GetBookingQueryHandler(ISqlConnectionFactory sqlConnectionFactory) : IQueryHandler<GetBookingQuery, GetBookingResponse>
{
    public async Task<Result<GetBookingResponse>> Handle(GetBookingQuery request, CancellationToken cancellationToken)
    {
        using var connection = sqlConnectionFactory.CreateConnection();

        const string sql = """
                           SELECT
                               "Id",
                               "ApartmentId",
                               "UserId",
                               "Status",
                               "PriceForPeriod_Amount" as "PriceAmount",
                               "PriceForPeriod_Currency" AS "PriceCurrency",
                               "CleaningFee_Amount" AS "CleaningFeeAmount",
                               "CleaningFee_Currency" AS "CleaningFeeCurrency",
                               "AmenitiesUpCharge_Amount" AS "AmenitiesUpChargeAmount",
                               "AmenitiesUpCharge_Currency" AS "AmenitiesUpChargeCurrency",
                               "TotalPrice_Amount" AS "TotalPriceAmount",
                               "TotalPrice_Currency" AS "TotalPriceCurrency",
                               "Duration_Start" AS "DurationStart",
                               "Duration_End" AS "DurationEnd",
                               "CreatedOnUtc" AS "CreatedOnUtc"
                           FROM bookings
                           WHERE "Id" = @BookingId
                           """;

        var booking = await connection.QueryFirstOrDefaultAsync<GetBookingResponse>(
            sql,
            new
            {
                request.BookingId
            });

        if (booking is null)
        {
            return Result.Failure<GetBookingResponse>(BookingErrors.NotFound);
        }

        return booking;
    }
}

public sealed record GetBookingQuery(Guid BookingId) : ICachedQuery<GetBookingResponse>
{
    public string CacheKey => $"Booking-{BookingId}";

    public TimeSpan? Expiration => null;
}


public record GetBookingResponse(
    Guid Id,
    Guid UserId,
    Guid ApartmentId,
    int Status,
    decimal PriceAmount,
    string PriceCurrency,
    decimal CleaningFeeAmount,
    string CleaningFeeCurrency,
    decimal AmenitiesUpChargeAmount,
    string AmenitiesUpChargeCurrency,
    decimal TotalPriceAmount,
    string TotalPriceCurrency,
    DateOnly DurationStart,
    DateOnly DurationEnd,
    DateTime CreatedOnUtc);
