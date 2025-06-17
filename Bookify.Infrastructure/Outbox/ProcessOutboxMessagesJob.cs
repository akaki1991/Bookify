using System.Data;
using Bookify.Application.Abstractions.Clock;
using Bookify.Application.Abstractions.Data;
using Bookify.Domain.Abstractions;
using Dapper;
using MediatR;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Quartz;

namespace Bookify.Infrastructure.Outbox;

[DisallowConcurrentExecution]
internal sealed class ProcessOutboxMessagesJob(
    ISqlConnectionFactory sqlConnectionFactory,
    IPublisher publisher,
    IDateTimeProvider dateTimeProvider,
    OutboxOptions outboxOptions,
    ILogger<ProcessOutboxMessagesJob> logger) : IJob
{
    private static readonly JsonSerializerSettings JsonSerializerSettings = new()
    {
        TypeNameHandling = TypeNameHandling.All
    };

    private readonly ISqlConnectionFactory _sqlConnectionFactory = sqlConnectionFactory;
    private readonly IPublisher _publisher = publisher;
    private readonly IDateTimeProvider _dateTimeProvider = dateTimeProvider;
    private readonly OutboxOptions _outboxOptions = outboxOptions;
    private readonly ILogger<ProcessOutboxMessagesJob> _logger = logger;

    public async Task Execute(IJobExecutionContext context)
    {
        _logger.LogInformation("Processing outbox messages...");

        using var connection = _sqlConnectionFactory.CreateConnection();
        using var transaction = connection.BeginTransaction();

        var outboxMessages = await GetOutboxMessagesAsync(connection, transaction);

        foreach (var outboxMessage in outboxMessages)
        {
            Exception? exception = null;

            try
            {
                var domainEvent = JsonConvert.DeserializeObject<IDomainEvent>(
                    outboxMessage.Content,
                    JsonSerializerSettings)!;

                await _publisher.Publish(domainEvent, context.CancellationToken);
            }
            catch (Exception caughtException)
            {
                _logger.LogError(
                    caughtException,
                    "Exception while processing outbox message {MessageId}",
                    outboxMessage.Id);

                exception = caughtException;
            }

            await UpdateOutboxMessageAsync(connection, transaction, outboxMessage, exception);
        }

        transaction.Commit();

        _logger.LogInformation("Completed processing outbox messages");
    }

    private async Task<IReadOnlyList<OutboxMessageResponse>> GetOutboxMessagesAsync(
        IDbConnection connection,
        IDbTransaction transaction)
    {
        var sql = $"""
                   SELECT "Id", "Content"
                   FROM "OutboxMessages"
                   WHERE "ProcessedOnUtc" IS NULL
                   ORDER BY "OccuredOnUtc"
                   LIMIT {_outboxOptions.BatchSize}
                   FOR UPDATE
                  """;

        var outboxMessages = await connection.QueryAsync<OutboxMessageResponse>(sql, transaction: transaction);

        return [.. outboxMessages];
    }

    private async Task UpdateOutboxMessageAsync(
       IDbConnection connection,
       IDbTransaction transaction,
       OutboxMessageResponse outboxMessage,
       Exception? exception)
    {
        const string sql = """
            UPDATE outbox_messages
            SET "ProcessedOnUtc" = @ProcessedOnUtc,
                error = @Error
            WHERE id = @Id
            """;

        await connection.ExecuteAsync(
            sql,
            new
            {
                outboxMessage.Id,
                ProcessedOnUtc = _dateTimeProvider.UtcNow,
                Error = exception?.ToString()
            },
            transaction: transaction);
    }

    internal sealed record OutboxMessageResponse(Guid Id, string Content);
}
