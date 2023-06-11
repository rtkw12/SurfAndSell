using Common.Caching;
using Common.MongoDb;
using Common.Util;
using Common.Util.Logging;
using MongoDB.Driver;

namespace UserEngine.Runtime;

public class StoreService : MongoService, IStoreService
{
    private readonly ICacheService _cacheService;
    private readonly ILoggingService _loggingService;

    public StoreService(IMongoClient client, ICacheService cacheService, ILoggingService loggingService) : base(client)
    {
        _cacheService = cacheService ?? throw new ArgumentNullException(nameof(cacheService));
        _loggingService = loggingService ?? throw new ArgumentNullException(nameof(loggingService));
    }

    private IMongoCollection<StoreStorage> Stores() =>
        GetDatabase(DatabaseNames.DATABASE).GetCollection<StoreStorage>(DatabaseNames.STORE);

    // Used for user to find store
    public async Task<IPaginatedView<IStore>> GetStores(IClientSessionHandle sessionHandle, CancellationToken cancellationToken, int page = 1,
        int pageSize = 50)
    {
        var fb = Builders<StoreStorage>.Filter;
        var filter = fb.Eq(x => x.Status, StoreStatus.OPEN);

        PipelineDefinition<StoreStorage, StoreStorage> pipeline = new EmptyPipelineDefinition<StoreStorage>();
        pipeline = pipeline.Match(filter);
        pipeline = pipeline.Skip((page - 1) * pageSize).Limit(pageSize);

        var count = await CountDocuments(sessionHandle, cancellationToken, StoreStatus.OPEN);
        var stores = await Stores().AggregateAsync(sessionHandle, pipeline, cancellationToken: cancellationToken);

        await stores.MoveNextAsync(cancellationToken);

        return new PaginatedView<IStore>(stores, new Pages(page, pageSize, count, Convert.ToInt32(count / pageSize)));
    }

    internal async Task<long> CountDocuments(IClientSessionHandle sessionHandle, CancellationToken cancellationToken, StoreStatus? status = null)
    {
        var fb = Builders<StoreStorage>.Filter;
        var filter = new List<FilterDefinition<StoreStorage>>();
        if (status != null)
        {
            filter.Add(fb.Eq(x => x.Status, status));
        }
        else
        {
            filter.Add(fb.Empty);
        }

        var count = await Stores().CountDocumentsAsync(sessionHandle, fb.And(filter), cancellationToken: cancellationToken);

        return count;
    }

    public async Task<TryResult<IStore>> TryGetStoreByUser(IClientSessionHandle sessionHandle, string userId, CancellationToken cancellationToken)
    {
        var fb = Builders<StoreStorage>.Filter;
        var filter = fb.Eq(x => x.UserId, userId);

        var store = await Stores().Find(sessionHandle, filter).FirstOrDefaultAsync(cancellationToken);

        return store != null
            ? TryResult<IStore>.Succeed(store)
            : TryResult<IStore>.Fail($"Could not find store belonging to user '{userId}'");
    }

    // Used by order engine to query for store item
    public async Task<TryResult<IStore>> TryGetStoreById(IClientSessionHandle sessionHandle, string id, CancellationToken cancellationToken)
    {
        var cache = await _cacheService.GetDataAsync<StoreStorage>(CacheKeyGenerator.Generate<StoreStorage>(id), TimeSpan.FromMinutes(3));
        if (cache != null)
        {
            return TryResult<IStore>.Succeed(cache);
        }

        var fb = Builders<StoreStorage>.Filter;
        var filter = fb.Eq(x => x.Id, id);

        var store = await Stores().Find(sessionHandle, filter).FirstOrDefaultAsync(cancellationToken);
        await _cacheService.SetDataAsync(CacheKeyGenerator.Generate<StoreStorage>(id), store, TimeSpan.FromMinutes(3));

        return store != null
            ? TryResult<IStore>.Succeed(store)
            : TryResult<IStore>.Fail($"Could not find store with id '{id}'");
    }

    public async Task<TryResult<IStore>> TryAddStore(IClientSessionHandle sessionHandle, IStoreInput input, CancellationToken cancellationToken)
    {
        var store = new StoreStorage(input, DateTime.UtcNow);
        await Stores().InsertOneAsync(sessionHandle, store, cancellationToken: cancellationToken);

        return TryResult<IStore>.Succeed(store);
    }

    public async Task<TryResult> TryUpdateStoreByUser(IClientSessionHandle sessionHandle, CancellationToken cancellationToken, string id, string userId,
        string? name = null, string? description = null, StoreStatus status = StoreStatus.OPEN)
    {
        var fb = Builders<StoreStorage>.Filter;
        var filter = fb.Eq(x => x.Id, id) & fb.Eq(x => x.UserId, userId);

        var ub = Builders<StoreStorage>.Update;
        var updates = new List<UpdateDefinition<StoreStorage>>();
        if (name != null)
        {
            updates.Add(ub.Set(x => x.Name, name));
        }
        if (description != null)
        {
            updates.Add(ub.Set(x => x.Name, name));
        }
        updates.Add(ub.Set(x => x.Status, status));

        var result = await Stores().UpdateOneAsync(sessionHandle, filter, ub.Combine(updates), cancellationToken: cancellationToken);
        await _cacheService.RemoveDataAsync(CacheKeyGenerator.Generate<StoreStorage>(id));

        return result.IsAcknowledged
            ? TryResult.Succeed()
            : TryResult.Fail($"Could not update store '{id}' owned by user '{userId}'");
    }
}