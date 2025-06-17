using Bookify.Application.Abstractions.Clock;
using Bookify.Application.Exceptions;
using Bookify.Domain.Abstractions;
using Bookify.Infrastructure.Outbox;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;

namespace Bookify.Infrastructure;

public sealed class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options, IDateTimeProvider dateTimeProvider)
    : DbContext(options), IUnitOfWork
{
    private readonly IDateTimeProvider _dateTimeProvider = dateTimeProvider;

    private static readonly JsonSerializerSettings JsonSerializerSettings = new()
    {
        TypeNameHandling = TypeNameHandling.All
    };

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);

        base.OnModelCreating(modelBuilder);
    }

    public override async Task<int> SaveChangesAsync(bool acceptAllChangesOnSuccess, CancellationToken cancellationToken = default)
    {
        try
        {
            AddDomainEventsAsOutboxMessages();
            var result = await base.SaveChangesAsync(acceptAllChangesOnSuccess, cancellationToken);

            return result;
        }
        catch (DbUpdateConcurrencyException ex)
        {

            throw new ConcurrencyException("Concurency exception occured", ex);
        }
    }

    public void AddDomainEventsAsOutboxMessages()
    {
        var outboxMessages = ChangeTracker
            .Entries<Entity>()
            .Select(e => e.Entity)
            .SelectMany(e =>
            {
                var domainEvents = e.GetDomainEvents();
                e.ClearDomainEvents();
                return domainEvents;
            })
            .Select(domainEvent => new OutboxMessage(
                Guid.NewGuid(),
                _dateTimeProvider.UtcNow,
                domainEvent.GetType().Name,
                JsonConvert.SerializeObject(domainEvent, JsonSerializerSettings)
                ))
            .ToArray();

        AddRange(outboxMessages);
    }
}