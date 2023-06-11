using Common.Caching;
using Common.Messaging.Interfaces;
using Common.Messaging.Models;
using Common.MongoDb;
using Common.Util;
using Common.Util.Logging;
using ItemEngine.Models;
using MongoDB.Bson;
using MongoDB.Driver;

namespace ItemEngine.Runtime;

public class ItemService : MongoService, IItemService
{
    private readonly ICacheService _cacheService;
    private readonly ILoggingService _loggingService;
    private readonly IPublisher _publisher;

    public ItemService(IMongoClient client, ICacheService cacheService, ILoggingService loggingService, IPublisher publisher) : base(client)
    {
        _cacheService = cacheService ?? throw new ArgumentNullException(nameof(cacheService));
        _loggingService = loggingService ?? throw new ArgumentNullException(nameof(loggingService));
        _publisher = publisher ?? throw new ArgumentNullException(nameof(publisher));
    }

    private IMongoCollection<ItemStorage> Items() =>
        GetDatabase(DatabaseNames.DATABASE).GetCollection<ItemStorage>(DatabaseNames.ITEM);

    public async Task<IPaginatedView<IItem>> GetItems(IClientSessionHandle session, CancellationToken cancellationToken, int page = 1, int pageSize = 50)
    {
        PipelineDefinition<ItemStorage, ItemStorage> pipeline = new EmptyPipelineDefinition<ItemStorage>();
        pipeline = pipeline.Match(Builders<ItemStorage>.Filter.Eq(x => x.Verified, true));
        pipeline = pipeline.Skip((page - 1) * pageSize).Limit(pageSize);

        var count = await CountDocuments(session, null, cancellationToken);
        var items = await Items().AggregateAsync(session, pipeline, cancellationToken: cancellationToken);

        var paginatedView =
            new PaginatedView<ItemStorage>(items, new Pages(page, pageSize, count, Convert.ToInt32(count / pageSize)));

        return new PaginatedView<IItem>(paginatedView.Cursor, paginatedView.Pagination);
    }

    public async Task<IPaginatedView<IItem>> GetItemsByStore(IClientSessionHandle sessionHandle, string storeId, CancellationToken cancellationToken, int page = 1, int pageSize = 50)
    {
        var cache = await _cacheService.GetDataAsync<PaginatedView<ItemStorage>>(
            CacheKeyGenerator.Generate<PaginatedView<ItemStorage>>(storeId, page.ToString()), TimeSpan.FromMinutes(5));

        if (cache != null)
        {
            return new PaginatedView<IItem>(cache.Cursor, cache.Pagination);
        }
        
        var fb = Builders<ItemStorage>.Filter;
        var filter = fb.Eq(x => x.StoreId, storeId) & fb.Eq(x => x.Verified, true);

        PipelineDefinition<ItemStorage, ItemStorage> pipeline = new EmptyPipelineDefinition<ItemStorage>();
        pipeline = pipeline.Match(filter);
        pipeline = pipeline.Skip((page - 1) * pageSize).Limit(pageSize);

        var count = await CountDocuments(sessionHandle, storeId, cancellationToken);
        var items = await Items().AggregateAsync(sessionHandle, pipeline, cancellationToken: cancellationToken);

        await items.MoveNextAsync(cancellationToken);

        var paginatedView =
            new PaginatedView<ItemStorage>(items, new Pages(page, pageSize, count, Convert.ToInt32(count / pageSize)));

        await _cacheService.SetDataAsync(
            CacheKeyGenerator.Generate<PaginatedView<ItemStorage>>(storeId, page.ToString()), 
            paginatedView,
            TimeSpan.FromMinutes(1));

        return new PaginatedView<IItem>(paginatedView.Cursor, paginatedView.Pagination);
    }

    internal async Task<long> CountDocuments(IClientSessionHandle sessionHandle, string? storeId, CancellationToken cancellationToken)
    {
        var fb = Builders<ItemStorage>.Filter;
        var filter = storeId != null ? fb.Eq(x => x.StoreId, storeId) & fb.Eq(x => x.Verified, true) : fb.Empty;

        return await Items().CountDocumentsAsync(sessionHandle, filter, cancellationToken: cancellationToken);
    }

    public async Task<TryResult<IItem>> TryGetItem(IClientSessionHandle sessionHandle, string storeId, string id, CancellationToken cancellationToken)
    {
        if (storeId == null) throw new ArgumentNullException(nameof(storeId));
        if (id == null) throw new ArgumentNullException(nameof(id));

        var cache =
            await _cacheService.GetDataAsync<ItemStorage>(
                CacheKeyGenerator.Generate<ItemStorage>(id),
                TimeSpan.FromSeconds(15));

        if (cache != null)
        {
            return TryResult<IItem>.Succeed(cache);
        }

        var fb = Builders<ItemStorage>.Filter;
        var filter = fb.Eq(x => x.Id, id) & fb.Eq(x => x.StoreId, storeId);

        var item = await Items().Find(sessionHandle, filter).FirstOrDefaultAsync(cancellationToken);

        return item != null
            ? TryResult<IItem>.Succeed(item)
            : TryResult<IItem>.Fail($"Could not find item with id {id}");
    }

    public async Task<TryResult<IItem>> TryAddItem(IClientSessionHandle sessionHandle, IItemInput input, CancellationToken cancellationToken)
    {
        if (input == null) throw new ArgumentNullException(nameof(input));

        var item = new ItemStorage(input, DateTime.UtcNow)
        {
            Id = ObjectId.GenerateNewId().ToString()
        };
        await Items().InsertOneAsync(sessionHandle, item, cancellationToken: cancellationToken);

        await _cacheService.SetDataAsync(
            CacheKeyGenerator.Generate<ItemStorage>(item.Id),
            item,
            TimeSpan.FromSeconds(15));

        _publisher.PublishResponse(new ItemStoreString(item.Id, item.StoreId), "ITEM.STORE", "ORDER.STORE", null);

        return TryResult<IItem>.Succeed(item);
    }

    public async Task<TryResult> TryAddQuantity(IClientSessionHandle sessionHandle, string storeId, string id, int quantity, CancellationToken cancellationToken)
    {
        var item = await TryGetItem(sessionHandle, storeId, id, cancellationToken);
        if (!item.Success)
        {
            return item;
        }

        if (item.Value.Quantity < quantity)
        {
            _publisher.Publish(new Item(item.Value), "ORDER_QUANTITY");

            var msg = $"Cannot deduct further than current amount '{item.Value.Quantity}' on item '{item.Value.Name}'";
            await _loggingService.LogErrorAsync(sessionHandle, msg, 400, cancellationToken);
            return TryResult.Fail(msg);
        }

        var fb = Builders<ItemStorage>.Filter;
        var filter = fb.Eq(x => x.Id, id) & fb.Eq(x => x.StoreId, storeId);

        var ub = Builders<ItemStorage>.Update;
        var update = ub.Inc(x => x.Quantity, quantity).Set(x => x.UpdatedAt, DateTime.UtcNow);

        var result = await Items().UpdateOneAsync(sessionHandle, filter, update, cancellationToken: cancellationToken);

        return result.IsAcknowledged
            ? TryResult.Succeed()
            : TryResult.Fail($"Could not update item quantity on item '{id}'");
    }

    public async Task<TryResult> TryUpdateItem(IClientSessionHandle sessionHandle, string storeId, string id, CancellationToken cancellationToken,
        string? name = null, string? description = null)
    {
        var fb = Builders<ItemStorage>.Filter;
        var filter = fb.Eq(x => x.Id, id) & fb.Eq(x => x.StoreId, storeId);

        var ub = Builders<ItemStorage>.Update;
        var updates = new List<UpdateDefinition<ItemStorage>>();
        if (name != null)
        {
            updates.Add(ub.Set(x => x.Name, name));
        }
        if (description != null)
        {
            updates.Add(ub.Set(x => x.Description, description));
        }
        updates.Add(ub.Set(x => x.UpdatedAt, DateTime.UtcNow));

        var result = await Items().UpdateOneAsync(sessionHandle, filter, ub.Combine(updates), cancellationToken: cancellationToken);

        return result.IsAcknowledged
            ? TryResult.Succeed()
            : TryResult.Fail($"Could not update on item '{id}'");
    }

    public async Task<TryResult> TryRemoveItem(IClientSessionHandle sessionHandle, string storeId, string id, CancellationToken cancellationToken)
    {
        var fb = Builders<ItemStorage>.Filter;
        var filter = fb.Eq(x => x.Id, id) & fb.Eq(x => x.StoreId, storeId);

        var result = await Items().DeleteOneAsync(sessionHandle, filter, cancellationToken: cancellationToken);

        var count = await CountDocuments(sessionHandle, storeId, cancellationToken);
        // Remove from cache
        await _cacheService.RemoveDataAsync(CacheKeyGenerator.Generate<ItemStorage>(id));
        for (var i = 0; i < count; i+=50)
        {
            await _cacheService.RemoveDataAsync(
                CacheKeyGenerator.Generate<PaginatedView<ItemStorage>>(storeId, (i / 50).ToString()));
        }

        return result.IsAcknowledged
            ? TryResult.Succeed()
            : TryResult.Fail($"Could not find item with id '{id}' in store '{storeId}'");
    }

    public async Task<bool> UpdateItemVerification(IClientSessionHandle sessionHandle, string storeId, string id, CancellationToken cancellationToken)
    {
        var fb = Builders<ItemStorage>.Filter;
        var filter = fb.Eq(x => x.Id, id) & fb.Eq(x => x.StoreId, storeId);

        var ub = Builders<ItemStorage>.Update;
        var update = ub.Set(x => x.UpdatedAt, DateTime.UtcNow)
            .Set(x => x.Verified, true);

        var result = await Items().UpdateOneAsync(sessionHandle, filter, update, cancellationToken: cancellationToken);

        return result.IsAcknowledged;
    }
}