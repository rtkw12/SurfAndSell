namespace UserEngine.Models;

public class StoreInput : IStoreInput
{
    public string UserId { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
}