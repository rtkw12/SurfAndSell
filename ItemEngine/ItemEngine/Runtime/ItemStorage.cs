using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace ItemEngine.Runtime;

internal class ItemStorage : IItem
{
    public ItemStorage() { }

    public ItemStorage(IItemInput input, DateTime now)
    {
        Name = input.Name;
        StoreId = input.StoreId;
        Description = input.Description;
        Quantity = input.Quantity;
        CreatedAt = now;
        UpdatedAt = now;
        Verified = false;
    }

    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string Id { get; set; }

    public string StoreId { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
    public int Quantity { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public bool Verified { get; set; }
}