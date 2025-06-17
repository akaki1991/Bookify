using Bookify.Domain.Abstractions;

namespace Bookify.Domain.UnitTests.Infrastructure;

public class BaseTests
{
    public static T AssertDomainEventWasPublished<T>(Entity entity)
        where T : IDomainEvent
    {
        var domainEvent = entity.GetDomainEvents().OfType<T>().SingleOrDefault();

        return domainEvent is null
            ? throw new Exception($"Expected domain event of type {typeof(T).Name} to be published, but it was not found.")
            : domainEvent;
    }
}
