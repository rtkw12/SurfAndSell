using System.Security.Cryptography;
using Common.Caching;
using Common.MongoDb;
using Common.Util;
using Common.Util.Logging;
using MongoDB.Bson;
using MongoDB.Driver;

namespace UserEngine.Runtime;

internal class UserService : MongoService, IUserService
{
    private readonly ILoggingService _loggingService;
    private readonly ICacheService _cacheService;
    private readonly TimeSpan _timeOut = TimeSpan.FromMinutes(3);

    public UserService(IMongoClient client, ILoggingService loggingService, ICacheService cacheService) : base(client)
    {
        _loggingService = loggingService ?? throw new ArgumentNullException(nameof(loggingService));
        _cacheService = cacheService ?? throw new ArgumentNullException(nameof(cacheService));
    }

    private IMongoCollection<UserStorage> Users() => GetDatabase(DatabaseNames.DATABASE).GetCollection<UserStorage>(DatabaseNames.USER);
    public async Task<TryResult<IUser>> TryGetUser(IClientSessionHandle sessionHandle, string id, CancellationToken cancellationToken)
    {
        if (id == null) throw new ArgumentNullException(nameof(id));
        
        // Check the cache first
        var result =
            await _cacheService.GetDataAsync<UserStorage>(CacheKeyGenerator.Generate<UserStorage>(id), _timeOut);
        if (result != null)
        {
            return TryResult<IUser>.Succeed(result);
        }

        var filter = Builders<UserStorage>.Filter.Eq(x => x.Id, id);

        var user = await Users().Find(sessionHandle, filter).FirstOrDefaultAsync(cancellationToken);

        if (user != null)
        {
            await _cacheService.SetDataAsync(CacheKeyGenerator.Generate<UserStorage>(user.Id), user, TimeSpan.FromMinutes(3));
            return TryResult<IUser>.Succeed(user);
        }

        return TryResult<IUser>.Fail($"Could not find user with id '{id}'");
    }

    public async Task<TryResult<IUser>> TryAddUser(IClientSessionHandle sessionHandle, IUserInput input, CancellationToken cancellationToken)
    {
        if (input == null) throw new ArgumentNullException(nameof(input));

        var builder = Builders<UserStorage>.Filter;
        var filter = builder.Eq(x => x.Email, input.Email);

        var user = await Users().Find(sessionHandle, filter).FirstOrDefaultAsync(cancellationToken);
        if (user != null)
        {
            return TryResult<IUser>.Fail($"Found a user with matching email '{input.Email}'");
        }

        var newUser = new UserStorage(input.Name, input.Email, BCrypt.Net.BCrypt.HashPassword(input.Password), DateTime.UtcNow)
        {
            Id = ObjectId.GenerateNewId().ToString()
        };
        await Users().InsertOneAsync(sessionHandle, newUser, cancellationToken: cancellationToken);

        await _cacheService.SetDataAsync(CacheKeyGenerator.Generate<UserStorage>(newUser.Id), newUser, TimeSpan.FromMinutes(3));

        return TryResult<IUser>.Succeed(newUser);
    }

    public async Task<TryResult> TryUpdateUser(IClientSessionHandle sessionHandle, 
        CancellationToken cancellationToken, 
        string id,
        string? name = null, 
        string? email = null, 
        string? password = null)
    {
        var fb = Builders<UserStorage>.Filter;
        var filter = fb.Eq(x => x.Id, id);

        var ub = Builders<UserStorage>.Update;
        var update = new List<UpdateDefinition<UserStorage>>
        {
            ub.Set(x => x.UpdatedAt, DateTime.UtcNow)
        };
        if (name != null)
        {
            update.Add(ub.Set(x => x.Name, name));
        }

        if (email != null)
        {
            update.Add(ub.Set(x => x.Email, email));
        }

        if (password != null)
        {
            update.Add(ub.Set(x => x.Password, BCrypt.Net.BCrypt.HashPassword(password)));
        }

        var result = await Users().UpdateOneAsync(sessionHandle, filter, ub.Combine(update), cancellationToken: cancellationToken);

        await _cacheService.RemoveDataAsync(CacheKeyGenerator.Generate<UserStorage>(id));

        return result.IsAcknowledged
            ? TryResult.Succeed()
            : TryResult.Fail($"Cannot update user with id '{id}'");
    }

    public async Task<TryResult<IUser>> Login(IClientSessionHandle sessionHandle, string email, string password, CancellationToken cancellationToken)
    {

        var builder = Builders<UserStorage>.Filter;
        var filter = builder.Eq(x => x.Email, email);

        var user = await Users().Find(sessionHandle, filter).FirstOrDefaultAsync(cancellationToken);
        if (user == null)
        {
            return TryResult<IUser>.Fail($"Could not find a user with matching email '{email}'");
        }

        if (user.Password != BCrypt.Net.BCrypt.HashPassword(password))
        {
            return TryResult<IUser>.Fail("Password is not the same");
        }

        return TryResult<IUser>.Succeed(user);
    }

    public async Task<TryResult> Logout(IClientSessionHandle sessionHandle, string id, CancellationToken cancellationToken)
    {
        var user = await TryGetUser(sessionHandle, id, cancellationToken);
        if (!user.Success)
        {
            return user;
        }

        var builder = Builders<UserStorage>.Filter;
        var filter = builder.Eq(x => x.Id, id);
        var update = Builders<UserStorage>.Update.Unset(x => x.Token);

        var result = await Users().UpdateOneAsync(sessionHandle, filter, update, cancellationToken: cancellationToken);

        return result.IsAcknowledged
            ? TryResult.Succeed()
            : TryResult.Fail($"Could not logout user with id {id}");
    }
}