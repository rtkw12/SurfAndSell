using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace UserEngine.Models;

public class User : IUser
{
    public User(IUser user)
    {
        if (user == null) throw new ArgumentNullException(nameof(user));
        
        Id = user.Id;
        Name = user.Name;
        Email = user.Email;
        CreatedAt = user.CreatedAt;
        UpdatedAt = user.UpdatedAt;
        Type = user.Type;
        Token = user.GetToken();
    }

    public string Id { get; }
    public string Name { get; }
    public string Email { get; }
    public DateTime CreatedAt { get; }
    public DateTime UpdatedAt { get; }

    [JsonConverter(typeof(StringEnumConverter))]
    public UserType Type { get; }
    public string? Token { get; }

    string? IUser.GetToken() => throw new NotSupportedException();
}