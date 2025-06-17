using Bookify.Application.Abstractions.Data;
using Bookify.Application.Abstractions.Messaging;
using Bookify.Domain.Abstractions;
using Bookify.Domain.Bookings;
using Dapper;

namespace Bookify.Application.Apartments.Queries;

internal sealed class SearchApartmentsQueryHandler(ISqlConnectionFactory sqlConnectionFactory) : IQueryHandler<SearchApartmentsQuery, IReadOnlyList<ApartmentResponse>>
{
    private static readonly int[] ActiveBookingStatuses =
    [
        (int)BookingStatus.Reserved,
        (int)BookingStatus.Confirmed,
        (int)BookingStatus.Completed
    ];

    public async Task<Result<IReadOnlyList<ApartmentResponse>>> Handle(SearchApartmentsQuery request, CancellationToken cancellationToken)
    {
        if (request.StartDate > request.EndDate)
        {
            return new List<ApartmentResponse>();
        }

        using var connection = sqlConnectionFactory.CreateConnection();

        const string sql = """
                                SELECT
                                    a."Id" AS Id,
                                    a."Name" AS Name,
                                    a."Description" AS Description,
                                    a."Price_Amount" AS Price,
                                    a."Price_Currency" AS Currency,
                                    a."Address_Country" AS Country,
                                    a."Address_State" AS State,
                                    a."Address_ZipCode" AS ZipCode,
                                    a."Address_City" AS City,
                                    a."Address_Street" AS Street
                                FROM "Apartments" AS a
                                WHERE NOT EXISTS
                                (
                                    SELECT 1
                                    FROM  bookings AS b
                                    WHERE
                                        b."ApartmentId" = a."Id" AND
                                        b."Duration_Start" <= @EndDate AND
                                        b."Duration_End" >= @StartDate AND
                                        b."Status" = ANY(@ActiveBookingStatuses)
                                )
                            """;


        var apartments = await connection
            .QueryAsync<ApartmentResponse, AddressResponse, ApartmentResponse>(
                sql,
                (apartment, address) =>
                {
                    apartment.Address = address;

                    return apartment;
                },
                new
                {
                    request.StartDate,
                    request.EndDate,
                    ActiveBookingStatuses
                },
                splitOn: "Country");

        return apartments.ToList();
    }
}

public sealed record SearchApartmentsQuery(
    DateOnly StartDate,
    DateOnly EndDate) : IQuery<IReadOnlyList<ApartmentResponse>>;

public class ApartmentResponse
{
    public Guid Id { get; set; }
    public string Name { get; set; } = default!;
    public string Description { get; set; } = default!;
    public decimal Price { get; set; }
    public string Currency { get; set; } = default!;
    public AddressResponse Address { get; set; } = default!;
}


public class AddressResponse
{
    public string Country { get; set; } = default!;
    public string State { get; set; } = default!;
    public string ZipCode { get; set; } = default!;
    public string City { get; set; } = default!;
    public string Street { get; set; } = default!;
}
