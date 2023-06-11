namespace ItemEngine.Models;

public class ItemQuantityUpdate
{
    public int Quantity { get; set; }
}

public class ItemUpdate
{
    public string Name { get; set; }
    public string Description { get; set; }
}

public class ItemInput : IItemInput
{
    public string StoreId { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
    public int Quantity { get; set; }
}