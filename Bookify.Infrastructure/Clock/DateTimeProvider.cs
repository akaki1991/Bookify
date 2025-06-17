using Bookify.Application.Abstractions.Clock;

namespace Bookify.Infrastructure.Clock;

public class DateTimeProvider : IDateTimeProvider
{
    public DateTime UtcNow => DateTime.UtcNow;
}
