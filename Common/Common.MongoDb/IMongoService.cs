using MongoDB.Driver;

namespace Common.MongoDb;

public interface IMongoService
{
    IMongoDatabase GetDatabase(string database);
    public IClientSessionHandle GetSessionHandle();
    public Task<IClientSessionHandle> GetSessionAsync();
}