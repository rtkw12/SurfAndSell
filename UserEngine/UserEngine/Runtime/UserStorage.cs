using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace UserEngine.Runtime;

internal class UserStorage : IUser
{
    public UserStorage() { }

    public UserStorage(string name, string email, string password, DateTime createdAt)
    {
        Name = name ?? throw new ArgumentNullException(nameof(name));
        Email = email ?? throw new ArgumentNullException(nameof(email));
        Password = password ?? throw new ArgumentNullException(nameof(password));
        CreatedAt = createdAt;
        UpdatedAt = createdAt;
        Token = null;
    }

    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string Id { get; set; }
    public string Name { get; set; }
    public string Email { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public string Password { get; set; }
    public string? Token { get; set; }

    public string? GetToken()
    {
        return Token;
    }
}