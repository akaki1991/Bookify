using Bookify.Application.Abstractions.Clock;
using Bookify.Application.Abstractions.Messaging;
using Bookify.Application.Exceptions;
using Bookify.Domain.Abstractions;
using Bookify.Domain.Apartments;
using Bookify.Domain.Bookings;
using Bookify.Domain.Users;
using FluentValidation;

namespace Bookify.Application.Bookings.Commands;

internal sealed class ReserveBookingCommandHandler(
    IUserRepository userRepository,
    IApartmentRepository apartmentRepository,
    IBookingRepository bookingRepository,
    IUnitOfWork unitOfWork,
    PricingService pricingService,
    IDateTimeProvider dateTimeProvider)
    : ICommandHandler<ReserveBookingCommand, Guid>
{
    public async Task<Result<Guid>> Handle(ReserveBookingCommand request, CancellationToken cancellationToken)
    {
        var user = await userRepository.GetByIdAsync(request.UserId, cancellationToken);

        if (user is null)
        {
            return Result.Failure<Guid>(UserErrors.NotFound);
        }

        var apartment = await apartmentRepository.GetByIdAsync(request.ApartmentId, cancellationToken);

        if (apartment is null)
        {
            return Result.Failure<Guid>(ApartmentErrors.NotFound);
        }

        var duration = DateRange.Create(request.StartDate, request.EnDate);

        if (await bookingRepository.IsOverlappingAsync(apartment, duration, cancellationToken))
        {
            return Result.Failure<Guid>(BookingErrors.Overlap);
        }

        try
        {
            var booking = Booking.Reserve(
            apartment,
            user.Id,
            duration,
            dateTimeProvider.UtcNow,
            pricingService);

            bookingRepository.Add(booking);
            await unitOfWork.SaveChangesAsync(cancellationToken);

            return booking.Id;
        }
        catch (ConcurrencyException)
        {
            return Result.Failure<Guid>(BookingErrors.Overlap);
        }
    }
}

public record ReserveBookingCommand(
    Guid ApartmentId,
    Guid UserId,
    DateOnly StartDate,
    DateOnly EnDate) : ICommand<Guid>;


public class ReserveBookingCommandValidator : AbstractValidator<ReserveBookingCommand>
{
    public ReserveBookingCommandValidator()
    {
        RuleFor(x => x.ApartmentId)
            .NotEmpty()
            .WithMessage("Apartment ID is required.");
        RuleFor(x => x.UserId)
            .NotEmpty()
            .WithMessage("User ID is required.");
        RuleFor(x => x.StartDate)
            .NotEmpty()
            .WithMessage("Start date is required.");
        RuleFor(x => x.EnDate)
            .NotEmpty()
            .WithMessage("End date is required.")
            .GreaterThan(x => x.StartDate)
            .WithMessage("End date must be greater than start date.");
    }
}
