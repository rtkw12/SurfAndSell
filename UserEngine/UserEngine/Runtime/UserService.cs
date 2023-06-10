using Common.MongoDb;
using Common.Util;
using Common.Util.Logging;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Driver;

namespace UserEngine.Runtime;

internal class UserService : MongoService, IUserService
{
    private readonly ILoggingService _loggingService;

    public UserService(IMongoClient client, ILoggingService loggingService) : base(client)
    {
        _loggingService = loggingService ?? throw new ArgumentNullException(nameof(loggingService));
    }

    private IMongoCollection<UserStorage> Users() => GetDatabase(DatabaseNames.DATABASE).GetCollection<UserStorage>(DatabaseNames.USER);
    public Task<TryResult<IUser>> TryGetUser(string id)
    {
        throw new NotImplementedException();
    }

    public Task<TryResult<IUser>> TryAddUser(IUserInput input)
    {
        throw new NotImplementedException();
    }

    public Task<TryResult<IUser>> TryUpdateUser(string? name = null, string? email = null, string? password = null)
    {
        throw new NotImplementedException();
    }
}

internal class UserStorage
{
    public UserStorage() { }

    public UserStorage(string name, string email, string password)
    {
        Name = name ?? throw new ArgumentNullException(nameof(name));
        Email = email ?? throw new ArgumentNullException(nameof(email));
        Password = password ?? throw new ArgumentNullException(nameof(password));
        Token = null;
    }

    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string Id { get; set; }
    public string Name { get; set; }
    public string Email { get; set; }
    public string Password { get; set; }
    public string? Token { get; set; }
}