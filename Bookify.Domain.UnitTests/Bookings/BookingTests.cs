using Bookify.Domain.Bookings;
using Bookify.Domain.Bookings.Events;
using Bookify.Domain.Shared;
using Bookify.Domain.UnitTests.Appartments;
using Bookify.Domain.UnitTests.Infrastructure;
using Bookify.Domain.UnitTests.Users;
using Bookify.Domain.Users;

namespace Bookify.Domain.UnitTests.Bookings;

public class BookingTests : BaseTests
{
    [Fact]
    public void Reserve_Should_RaiseBookingReservedDomainEvent()
    {
        // Arrange
        var user = User.Create(UserData.FirstName, UserData.lastName, UserData.Email);
        var price = new Money(10.0m, Currency.Usd);
        var period = DateRange.Create(new DateOnly(2024, 1, 1), new DateOnly(2024, 1, 10));
        var apartment = ApartmentData.Create(price);
        var pricingService = new PricingService();

        // Act
        var booking = Booking.Reserve(apartment, user.Id, period, DateTime.UtcNow, pricingService);
        
        // Assert

        var domainEvent = AssertDomainEventWasPublished<BookingReservedDomainEvent>(booking);
        Assert.Equal(booking.Id, domainEvent.BookingId);
    }
}
