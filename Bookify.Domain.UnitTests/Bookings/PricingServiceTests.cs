using Bookify.Domain.Bookings;
using Bookify.Domain.Shared;
using Bookify.Domain.UnitTests.Appartments;

namespace Bookify.Domain.UnitTests.Bookings;

public class PricingServiceTests
{
    [Fact]
    public void CalculatePrice_Should_ReturnCorrectTotalPrice()
    {
        // Arrange
        var price = new Money(10.0m, Currency.Usd);
        var period = DateRange.Create(new DateOnly(2024, 1, 1), new DateOnly(2024, 1, 10));
        var expectedTotalPrice = new Money(price.Amount * period.LengthInDays, Currency.Usd);
        var apartment = ApartmentData.Create(price);
        var pricingService = new PricingService();

        // Act
        var pricingDetails = pricingService.CalculatePrice(apartment, period);

        // Assert
        Assert.Equal(expectedTotalPrice, pricingDetails.TotalPrice);
    }

    [Fact]
    public void CalculatePrice_Should_ReturnCorrectTotalPrice_WhenCleaningFeeIsIncluded()
    {
        // Arrange
        var price = new Money(10.0m, Currency.Usd);
        var cleaningFee = new Money(99.99m, Currency.Usd);
        var period = DateRange.Create(new DateOnly(2024, 1, 1), new DateOnly(2024, 1, 10));
        var expectedTotalPrice = new Money(price.Amount * period.LengthInDays + cleaningFee.Amount, Currency.Usd);
        var apartment = ApartmentData.Create(price, cleaningFee: cleaningFee);
        var pricingService = new PricingService();
        // Act
        var pricingDetails = pricingService.CalculatePrice(apartment, period);
        // Assert
        Assert.Equal(expectedTotalPrice, pricingDetails.TotalPrice);
    }
}
