﻿using Bookify.Domain.Apartments;
using Bookify.Domain.Shared;

namespace Bookify.Domain.UnitTests.Appartments;

internal static class ApartmentData
{
    public static Apartment Create(Money price, Money? cleaningFee = null) => new(
            Guid.NewGuid(),
            new Name("Test Appartment"),
            new Description("Test Description"),
            new Address("Country", "State", "ZipCode", "City", "Street"),
            price,
            cleaningFee ?? Money.Zero(),
            []);
}
