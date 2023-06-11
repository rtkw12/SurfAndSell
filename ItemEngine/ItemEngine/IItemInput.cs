namespace ItemEngine;

public interface IItemInput
{
    string StoreId { get; }
    string Name { get; }
    string Description { get; }
    int Quantity { get; }
}