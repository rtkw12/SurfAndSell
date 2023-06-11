namespace ItemEngine;

public interface IItem
{
    string Id { get; }
    string StoreId { get; }
    string Name { get; }
    string Description { get; }
    int Quantity { get; }
    DateTime CreatedAt { get; }
    DateTime UpdatedAt { get; }
}