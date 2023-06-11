using Common.MongoDb;
using Common.Util;
using MongoDB.Driver;

namespace ItemEngine;

public interface IItemService : IMongoService
{
    Task<IPaginatedView<IItem>> GetItems(IClientSessionHandle session, CancellationToken cancellationToken,
        int page = 1, int pageSize = 50);
    Task<IPaginatedView<IItem>> GetItemsByStore(IClientSessionHandle sessionHandle, string storeId, CancellationToken cancellationToken, int page = 1, int pageSize = 50);
    Task<TryResult<IItem>> TryGetItem(IClientSessionHandle sessionHandle, string storeId, string id, CancellationToken cancellationToken);
    Task<TryResult<IItem>> TryAddItem(IClientSessionHandle sessionHandle, IItemInput input, CancellationToken cancellationToken);
    Task<TryResult> TryAddQuantity(IClientSessionHandle sessionHandle, string storeId, string id, int quantity, CancellationToken cancellationToken);
    Task<TryResult> TryUpdateItem(IClientSessionHandle sessionHandle, string storeId, string id, CancellationToken cancellationToken, string? name = null, string? description = null);
    Task<TryResult> TryRemoveItem(IClientSessionHandle sessionHandle, string storeId, string id, CancellationToken cancellationToken);
    Task<bool> UpdateItemVerification(IClientSessionHandle sessionHandle, string storeId, string id, CancellationToken cancellationToken);
}