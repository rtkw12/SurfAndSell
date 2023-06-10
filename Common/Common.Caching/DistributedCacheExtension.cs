using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
using Common.Util;
using Microsoft.Extensions.Caching.Distributed;

namespace Common.Caching;

public static class DistributedCacheExtension
{
    public static async Task SetRecordAsync<T>(this IDistributedCache cache,
        string recordId,
        [DisallowNull] T data,
        TimeSpan? absoluteExpirationTime = null,
        TimeSpan? unusedExpirationTime = null,
        CancellationToken? cancellationToken = null)
    {
        if (cache == null) throw new ArgumentNullException(nameof(cache));
        if (recordId == null) throw new ArgumentNullException(nameof(recordId));
        if (data == null) throw new ArgumentNullException(nameof(data));

        // Expiration setup
        var options = new DistributedCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = absoluteExpirationTime ?? TimeSpan.FromSeconds(60),
            SlidingExpiration = unusedExpirationTime
        };

        var token = cancellationToken ?? CancellationToken.None;

        var jsonData = JsonSerializer.Serialize(data);
        
        // Set the cache
        await cache.SetStringAsync(recordId, jsonData, options, token);
    }

    public static async Task<TryResult<T>> TryGetRecordAsync<T>(this IDistributedCache cache, string recordId)
    {
        if (cache == null) throw new ArgumentNullException(nameof(cache));
        if (recordId == null) throw new ArgumentNullException(nameof(recordId));

        var jsonData = await cache.GetStringAsync(recordId);

        if (jsonData == null)
        {
            return TryResult<T>.Fail($"Could not find data in cache with record id '{recordId}'");
        }

        var data = JsonSerializer.Deserialize<T>(jsonData);

        if (data == null)
        {
            throw new InvalidCastException(
                $"The data retrieved could not be cast into '{typeof(T)}', check that it is correct");
        }

        return TryResult<T>.Succeed(data);
    }
}