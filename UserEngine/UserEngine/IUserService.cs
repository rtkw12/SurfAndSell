using Common.MongoDb;
using Common.Util;
using MongoDB.Driver;

namespace UserEngine;

public interface IUserService : IMongoService
{
    Task<TryResult<IUser>> TryGetUser(IClientSessionHandle sessionHandle, string id, CancellationToken cancellationToken);
    Task<TryResult<IUser>> TryAddUser(IClientSessionHandle sessionHandle, IUserInput input, CancellationToken cancellationToken);
    Task<TryResult> TryUpdateUser(IClientSessionHandle sessionHandle, CancellationToken cancellationToken, string id, string? name = null, string? email = null, string? password = null);
    Task<TryResult<IUser>> Login(IClientSessionHandle sessionHandle, string email, string password, CancellationToken cancellationToken);
    Task<TryResult> Logout(IClientSessionHandle sessionHandle, string id, CancellationToken cancellationToken);
}