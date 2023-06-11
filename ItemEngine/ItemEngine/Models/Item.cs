namespace ItemEngine.Models;

public class Item : IItem
{
    public Item(IItem item)
    {
        Id = item.Id;
        StoreId = item.StoreId;
        Name = item.Name;
        Description = item.Description;
        Quantity = item.Quantity;
        CreatedAt = item.CreatedAt;
        UpdatedAt = item.UpdatedAt;
    }

    public string Id { get; }
    public string StoreId { get; }
    public string Name { get; }
    public string Description { get; }
    public int Quantity { get; }
    public DateTime CreatedAt { get; }
    public DateTime UpdatedAt { get; }
}