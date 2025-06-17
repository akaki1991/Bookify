using Bookify.Application.Abstractions.Caching;
using Bookify.Domain.Abstractions;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Bookify.Application.Abstractions.Behaviors;

internal sealed class QueryCacheingBehavior<TRequest, TResponse>(ICacheService cache, ILogger<QueryCacheingBehavior<TRequest, TResponse>> logger)
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : ICachedQuery
    where TResponse : Result
{
    private readonly ICacheService _cache = cache;
    private readonly ILogger<QueryCacheingBehavior<TRequest, TResponse>> _logger = logger;

    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        TResponse? cachedResult = await _cache.GetAsync<TResponse>(request.CacheKey, cancellationToken);

        var name = typeof(TRequest).Name;

        if (cachedResult is not null)
        {
            _logger.LogInformation("Request {Request} processed from cache", name);
            return cachedResult;
        }

        _logger.LogInformation("Request {Request} processed without cache", name);

        var result = await next(cancellationToken);

        if (result.IsSuccess)
        {
            await _cache.SetAsync(request.CacheKey, result, request.Expiration, cancellationToken);
        }

        return result;
    }
}
