namespace UserEngine;

public interface IStoreInput
{
    string UserId { get; }
    string Name { get; }
    string Description { get; }
}