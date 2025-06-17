using Bookify.Application.Apartments.Queries;
using Bookify.Application.IntegrationTests.Infrastructure;

namespace Bookify.Application.IntegrationTests.Apartments;

public class SearchApartmentsTests(IntegrationTestWebAppFactory factory) : BaseIntegrationTest(factory)
{
    [Fact]
    public async Task SearchApartments_ShouldReturnAvailableApartments_WhenDatesAreValid()
    {
        // Arrange
        var startDate = new DateOnly(2024, 1, 1);
        var endDate = new DateOnly(2024, 1, 10);
        // Act
        var result = await Sender.Send(new SearchApartmentsQuery(startDate, endDate));
        // Assert
        Assert.True(result.IsSuccess);
        Assert.NotEmpty(result.Value);
    }

    [Fact]
    public async Task SearchApartments_ShouldReturnEmptyList_WhenDatesAreInvalid()
    {
        // Arrange
        var startDate = new DateOnly(2024, 1, 10);
        var endDate = new DateOnly(2024, 1, 1);
        // Act
        var result = await Sender.Send(new SearchApartmentsQuery(startDate, endDate));

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Empty(result.Value);
    }
}
