using Bookify.Application.Bookings.Queries;
using Bookify.Application.IntegrationTests.Infrastructure;
using Bookify.Domain.Bookings;

namespace Bookify.Application.IntegrationTests.Bookings;

public class GetbookingTests(IntegrationTestWebAppFactory factory) : BaseIntegrationTest(factory)
{ 
    private static readonly Guid BookingId = Guid.NewGuid();

    [Fact]
    public async Task GetBooking_ShouldReturnFailure_WhenBookingIsNotFound()
    {
        // Arrange
        var query = new GetBookingQuery(BookingId);

        // Act
        var result = await Sender.Send(query);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(BookingErrors.NotFound, result.Error);
    }
}
