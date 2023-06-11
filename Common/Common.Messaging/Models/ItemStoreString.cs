using Newtonsoft.Json;

namespace Common.Messaging.Models;

public class ItemStoreString
{
    [JsonConstructor]
    public ItemStoreString(string itemId, string storeId)
    {
        ItemId = itemId ?? throw new ArgumentNullException(nameof(itemId));
        StoreId = storeId ?? throw new ArgumentNullException(nameof(storeId));
    }

    public string ItemId { get; }
    public string StoreId { get; }
}