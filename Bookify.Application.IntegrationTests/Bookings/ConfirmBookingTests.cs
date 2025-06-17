using Bookify.Application.Bookings.Commands;
using Bookify.Application.IntegrationTests.Infrastructure;
using Bookify.Domain.Bookings;

namespace Bookify.Application.IntegrationTests.Bookings;

public class ConfirmBookingTests(IntegrationTestWebAppFactory factory) : BaseIntegrationTest(factory)
{
    [Fact]
    public async Task ConfirmBooking_ShouldReturnFailure_WhenBookingIsNotFound()
    {
        // Arrange
        var bookingId = Guid.NewGuid(); // Replace with a valid booking ID
        var command = new ConfirmBookingCommand(bookingId);
        // Act
        var result = await Sender.Send(command);
        
        // Assert
        Assert.Equal(BookingErrors.NotFound, result.Error);
    }
}
