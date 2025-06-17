using Bookify.Application.Abstractions.Clock;
using Bookify.Application.Abstractions.Messaging;
using Bookify.Domain.Abstractions;
using Bookify.Domain.Bookings;

namespace Bookify.Application.Bookings.Commands;

internal sealed class ConfirmBookingCommandHandler(
    IDateTimeProvider dateTimeProvider,
    IBookingRepository bookingRepository,
    IUnitOfWork unitOfWork) : ICommandHandler<ConfirmBookingCommand>
{
    private readonly IDateTimeProvider _dateTimeProvider = dateTimeProvider;
    private readonly IBookingRepository _bookingRepository = bookingRepository;
    private readonly IUnitOfWork _unitOfWork = unitOfWork;

    public async Task<Result> Handle(
        ConfirmBookingCommand request,
        CancellationToken cancellationToken)
    {
        var booking = await _bookingRepository.GetByIdAsync(request.BookingId, cancellationToken);

        if (booking is null)
        {
            return Result.Failure(BookingErrors.NotFound);
        }

        var result = booking.Confirm(_dateTimeProvider.UtcNow);

        if (result.IsFailure)
        {
            return result;
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}

public sealed record ConfirmBookingCommand(Guid BookingId) : ICommand;