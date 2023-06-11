namespace UserEngine;

public interface IStore
{
    string Id { get; }
    string UserId { get; }
    string Name { get; }
    string Description { get; }
    StoreStatus Status { get; }
    DateTime CreatedAt { get; }
    DateTime UpdatedAt { get; }
}