using Common.MongoDb;
using Common.Util;
using MongoDB.Driver;

namespace UserEngine;

public interface IStoreService : IMongoService
{
    Task<IPaginatedView<IStore>> GetStores(IClientSessionHandle sessionHandle, CancellationToken cancellationToken,
        int page = 1, int pageSize = 50); 
    Task<TryResult<IStore>> TryGetStoreByUser(IClientSessionHandle sessionHandle, string userId, CancellationToken cancellationToken);
    Task<TryResult<IStore>> TryGetStoreById(IClientSessionHandle sessionHandle, string id, CancellationToken cancellationToken);
    Task<TryResult<IStore>> TryAddStore(IClientSessionHandle sessionHandle, IStoreInput input, CancellationToken cancellationToken);
    Task<TryResult> TryUpdateStoreByUser(IClientSessionHandle sessionHandle, CancellationToken cancellationToken, string id, string userId, string? name = null, string? description = null, StoreStatus status = StoreStatus.OPEN);
}