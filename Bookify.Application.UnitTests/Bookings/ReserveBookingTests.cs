using Bookify.Application.Abstractions.Clock;
using Bookify.Application.Bookings.Commands;
using Bookify.Application.Exceptions;
using Bookify.Application.UnitTests.Apartments;
using Bookify.Application.UnitTests.Users;
using Bookify.Domain.Abstractions;
using Bookify.Domain.Apartments;
using Bookify.Domain.Bookings;
using Bookify.Domain.Users;
using NSubstitute;
using NSubstitute.ExceptionExtensions;

namespace Bookify.Application.UnitTests.Bookings;

public class ReserveBookingTests
{
    private static readonly DateTime UtcNow = DateTime.UtcNow;
    private static readonly ReserveBookingCommand Command = new(
        Guid.NewGuid(),
        Guid.NewGuid(),
        new DateOnly(2024, 1, 1),
        new DateOnly(2024, 1, 10));

    private readonly ReserveBookingCommandHandler _handler;

    private readonly IUserRepository userRepositoryMock;
    private readonly IApartmentRepository apartmentRepositoryMock;
    private readonly IBookingRepository bookingRepositoryMock;
    private readonly IUnitOfWork unitOfWorkMock;
    private readonly PricingService pricingServiceMock;
    private readonly IDateTimeProvider dateTimeProviderMock;

    public ReserveBookingTests()
    {
        userRepositoryMock = Substitute.For<IUserRepository>();
        apartmentRepositoryMock = Substitute.For<IApartmentRepository>();
        bookingRepositoryMock = Substitute.For<IBookingRepository>();
        unitOfWorkMock = Substitute.For<IUnitOfWork>();
        pricingServiceMock = new PricingService();

        dateTimeProviderMock = Substitute.For<IDateTimeProvider>();
        dateTimeProviderMock.UtcNow.Returns(UtcNow);

        _handler = new ReserveBookingCommandHandler(
            userRepositoryMock,
            apartmentRepositoryMock,
            bookingRepositoryMock,
            unitOfWorkMock,
            pricingServiceMock,
            dateTimeProviderMock);
    }

    [Fact]
    public async Task Handle_Should_ReturnFailure_WhenUserIsNull()
    {
        // Arrange
        userRepositoryMock.GetByIdAsync(Command.UserId, Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<User?>(null));

        // Act

        var result = await _handler.Handle(Command, CancellationToken.None);

        // Assert

        Assert.Equal(result.Error, UserErrors.NotFound);
    }

    [Fact]
    public async Task Handle_Should_ReturnFailure_WhenApartmentIsNull()
    {
        // Arrange
        userRepositoryMock.GetByIdAsync(Command.UserId, Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<User?>(UserData.Create()));

        apartmentRepositoryMock.GetByIdAsync(Command.ApartmentId, Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Apartment?>(null));

        // Act
        var result = await _handler.Handle(Command, CancellationToken.None);

        // Assert
        Assert.Equal(result.Error, ApartmentErrors.NotFound);
    }


    [Fact]
    public async Task Handle_Should_ReturnFailure_WhenApartmentIsBooked()
    {
        // Arrange
        var apartment = ApartmentData.Create();
        var duration = DateRange.Create(Command.StartDate, Command.EnDate);

        userRepositoryMock.GetByIdAsync(Command.UserId, Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<User?>(UserData.Create()));

        apartmentRepositoryMock.GetByIdAsync(Command.ApartmentId, Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Apartment?>(apartment));

        bookingRepositoryMock.IsOverlappingAsync(apartment, duration, Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(true));

        // Act
        var result = await _handler.Handle(Command, CancellationToken.None);

        // Assert
        Assert.Equal(result.Error, BookingErrors.Overlap);
    }

    [Fact]
    public async Task Handle_Should_ReturnFailure_WhenUnitOfWorkkThrows()
    {
        //Arrange
        var user = UserData.Create();
        var apartment = ApartmentData.Create();
        var duration = DateRange.Create(Command.StartDate, Command.EnDate);

        userRepositoryMock.GetByIdAsync(Command.UserId, Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<User?>(user));

        apartmentRepositoryMock.GetByIdAsync(Command.ApartmentId, Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Apartment?>(apartment));

        bookingRepositoryMock.IsOverlappingAsync(apartment, duration, Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(false));

        unitOfWorkMock.SaveChangesAsync()
            .ThrowsAsync(new ConcurrencyException("Concurency", new Exception()));

        //Act

        var result = await _handler.Handle(Command, CancellationToken.None);

        //Assert
        Assert.Equal(result.Error, BookingErrors.Overlap);
    }



    [Fact]
    public async Task Handle_Should_ReturnSuccess_WhenBookingIsReserved()
    {
        //Arrange
        var user = UserData.Create();
        var apartment = ApartmentData.Create();
        var duration = DateRange.Create(Command.StartDate, Command.EnDate);

        userRepositoryMock.GetByIdAsync(Command.UserId, Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<User?>(user));

        apartmentRepositoryMock.GetByIdAsync(Command.ApartmentId, Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Apartment?>(apartment));

        bookingRepositoryMock.IsOverlappingAsync(apartment, duration, Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(false));

        //Act

        var result = await _handler.Handle(Command, CancellationToken.None);

        //Assert
        Assert.True(result.IsSuccess);
    }

    [Fact]
    public async Task Handle_Should_CallRepository_WhenBookingIsReserved()
    {
        //Arrange
        var user = UserData.Create();
        var apartment = ApartmentData.Create();
        var duration = DateRange.Create(Command.StartDate, Command.EnDate);

        userRepositoryMock.GetByIdAsync(Command.UserId, Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<User?>(user));

        apartmentRepositoryMock.GetByIdAsync(Command.ApartmentId, Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Apartment?>(apartment));

        bookingRepositoryMock.IsOverlappingAsync(apartment, duration, Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(false));

        //Act

        var result = await _handler.Handle(Command, CancellationToken.None);

        //Assert
        bookingRepositoryMock.Received(1).Add(Arg.Is<Booking>(b =>
            b.Id == result.Value));
    }
}
