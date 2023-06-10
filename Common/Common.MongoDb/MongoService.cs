using MongoDB.Driver;

namespace Common.MongoDb
{
    public abstract class MongoService : IMongoService
    {
        private readonly IMongoClient _client;

        protected MongoService(IMongoClient client)
        {
            _client = client ?? throw new ArgumentNullException(nameof(client));
        }

        public IMongoDatabase GetDatabase(string database)
        {
            return _client.GetDatabase(database);
        }

        public IClientSessionHandle GetSessionHandle()
        {
            return _client.StartSession();
        }

        public async Task<IClientSessionHandle> GetSessionAsync()
        {
            return await _client.StartSessionAsync();
        }
    }
}