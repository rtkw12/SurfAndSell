using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace UserEngine.Models;

public class Store : IStore
{
    public Store(IStore store)
    {
        if (store == null) throw new ArgumentNullException(nameof(store));
        
        Id = store.Id;
        UserId = store.UserId;
        Name = store.Name;
        Description = store.Description;
        Status = store.Status;
        CreatedAt = store.CreatedAt;
        UpdatedAt = store.UpdatedAt;
    }

    public string Id { get; }
    public string UserId { get; }
    public string Name { get; }
    public string Description { get; }

    [JsonConverter(typeof(StringEnumConverter))]
    public StoreStatus Status { get; }
    public DateTime CreatedAt { get; }
    public DateTime UpdatedAt { get; }
}