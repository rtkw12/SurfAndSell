using System.ComponentModel.Design;
using System.Text.Json;
using Common.Util.Logging;
using MongoDB.Driver;
using StackExchange.Redis;

namespace Common.Caching;

public interface ICacheConnectionProvider
{
    string ConnectionString { get; }
}

public class CacheConnectionProvider : ICacheConnectionProvider
{
    public CacheConnectionProvider(string connectionString)
    {
        ConnectionString = connectionString ?? throw new ArgumentNullException(nameof(connectionString));
    }

    public string ConnectionString { get; }
}

public interface ICacheService
{
    T? GetData<T>(string key, TimeSpan? slidingExpiration = null);
    object RemoveData(string key);
    bool SetData<T>(string key, T value, TimeSpan? expirationTime = null);

    Task<T?> GetDataAsync<T>(string key, TimeSpan? slidingExpiration = null);
    Task<object> RemoveDataAsync(string key);
    Task<bool> SetDataAsync<T>(string key, T value, TimeSpan? expirationTime = null);
}

public class CacheService : ICacheService
{
    private readonly IDatabase _cacheDb;

    public CacheService(ICacheConnectionProvider cacheConnectionProvider)
    {
        var redis = ConnectionMultiplexer.Connect(cacheConnectionProvider.ConnectionString);
        _cacheDb = redis.GetDatabase();
    }

    public T? GetData<T>(string key, TimeSpan? slidingExpiration = null)
    {
        var value = _cacheDb.StringGet(key);
        if (string.IsNullOrEmpty(value))
        {
            _cacheDb.KeyExpire(key, slidingExpiration ?? TimeSpan.FromSeconds(60));
            return JsonSerializer.Deserialize<T>(value);
        }

        return default;
    }

    public object RemoveData(string key)
    {
        var exist = _cacheDb.KeyExists(key);

        if (exist)
        {
            return _cacheDb.KeyDelete(key);
        }
        
        return false;
    }

    public bool SetData<T>(string key, T value, TimeSpan? expirationTime = null)
    {
        var expiryTime = expirationTime ?? TimeSpan.FromSeconds(60);
        return _cacheDb.StringSet(key, JsonSerializer.Serialize(value), expiryTime);
    }

    public async Task<T?> GetDataAsync<T>(string key, TimeSpan? slidingExpiration = null)
    {
        var value = await _cacheDb.StringGetAsync(key);
        if (string.IsNullOrEmpty(value))
        {
            await _cacheDb.KeyExpireAsync(key, slidingExpiration ?? TimeSpan.FromSeconds(60));
            return JsonSerializer.Deserialize<T>(value);
        }

        return default;
    }

    public async Task<object> RemoveDataAsync(string key)
    {
        var exist = await _cacheDb.KeyExistsAsync(key);

        if (exist)
        {
            return await _cacheDb.KeyDeleteAsync(key);
        }

        return false;
    }

    public async Task<bool> SetDataAsync<T>(string key, T value, TimeSpan? expirationTime = null)
    {
        var expiryTime = expirationTime ?? TimeSpan.FromSeconds(60);
        return await _cacheDb.StringSetAsync(key, JsonSerializer.Serialize(value), expiryTime);
    }
}