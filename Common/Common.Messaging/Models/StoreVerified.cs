namespace Common.Messaging.Models;

public class StoreVerified : ItemStoreString
{
    public StoreVerified(ItemStoreString itemStore, bool verified) : base(itemStore.ItemId, itemStore.StoreId)
    {
        Verified = verified;
    }
    public bool Verified { get; }
}