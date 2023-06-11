using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace UserEngine.Models;

public class StoreUpdate
{
    public string? Name { get; set; }
    public string? Description { get; set; }

    [JsonConverter(typeof(StringEnumConverter))]
    public StoreStatus? Status { get; set; }
}