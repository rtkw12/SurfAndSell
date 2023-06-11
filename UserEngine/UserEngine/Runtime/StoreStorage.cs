using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace UserEngine.Runtime;

internal class StoreStorage : IStore
{
    public StoreStorage() { }

    public StoreStorage(IStoreInput input, DateTime now)
    {
        UserId = input.UserId;
        Name = input.Name;
        Description = input.Description;
        Status = StoreStatus.OPEN;
        CreatedAt = now;
        UpdatedAt = now;
    }

    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string Id { get; set; }
    public string UserId { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
    public StoreStatus Status { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}