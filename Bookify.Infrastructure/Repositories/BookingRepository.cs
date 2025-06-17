using Bookify.Domain.Apartments;
using Bookify.Domain.Bookings;
using Microsoft.EntityFrameworkCore;

namespace Bookify.Infrastructure.Repositories;

internal sealed class BookingRepository(ApplicationDbContext dbContext) : Repository<Booking>(dbContext), IBookingRepository
{
    private static readonly BookingStatus[] ActiveBookingStatuses =
    [
        BookingStatus.Reserved,
        BookingStatus.Confirmed,
        BookingStatus.Rejected
    ];

    public Task<bool> IsOverlappingAsync(
        Apartment apartment,
        DateRange duration, 
        CancellationToken cancellationToken = default) =>
        _dbContext.Set<Booking>()
            .AnyAsync(
                x => x.ApartmentId == apartment.Id
                     && x.Duration.Start <= duration.End
                     && ActiveBookingStatuses.Contains(x.Status),
                cancellationToken);
}