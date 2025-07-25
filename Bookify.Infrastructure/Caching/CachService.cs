﻿using System.Buffers;
using System.Text.Json;
using Bookify.Application.Abstractions.Caching;
using Microsoft.Extensions.Caching.Distributed;

namespace Bookify.Infrastructure.Caching;

internal sealed class CachService(IDistributedCache cache) : ICacheService
{
    private readonly IDistributedCache _cache = cache;

    public async Task<T?> GetAsync<T>(string key, CancellationToken cancellationToken = default)
    {
        var result = await _cache.GetAsync(key, cancellationToken);

        return result is null ? default : Deserialize<T>(result);
    }

    public Task SetAsync<T>(
        string key,
        T value,
        TimeSpan? expiration = null,
        CancellationToken cancellationToken = default)
    {
        var bytes = Serialize(value);

        return _cache.SetAsync(key, bytes, CacheOptions.Create(expiration), cancellationToken);
    }

    public Task RemoveAsync(string key, CancellationToken cancellationToken = default)
    {
        return _cache.RemoveAsync(key, cancellationToken);
    }

    public static T Deserialize<T>(byte[] bytes) => JsonSerializer.Deserialize<T>(bytes)!;

    public static byte[] Serialize<T>(T value)
    {
        var buffer = new ArrayBufferWriter<byte>();

        using var writer = new Utf8JsonWriter(buffer);

        JsonSerializer.Serialize(writer, value);

        return buffer.WrittenSpan.ToArray();
    }
}

public static class CacheOptions
{
    public static DistributedCacheEntryOptions DefaultExpiration =
        new()
        {
            AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(1)
        };

    public static DistributedCacheEntryOptions Create(TimeSpan? expiration) =>
        expiration is null
            ? DefaultExpiration
            : new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = expiration
            };
}
