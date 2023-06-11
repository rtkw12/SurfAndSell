namespace UserEngine.Models;

public class User : IUser
{
    private readonly IUser _user;

    public User(IUser user)
    {
        _user = user ?? throw new ArgumentNullException(nameof(user));
    }

    public string Id => _user.Id;
    public string Name => _user.Name;
    public string Email => _user.Email;
    public DateTime CreatedAt => _user.CreatedAt;
    public DateTime UpdatedAt => _user.UpdatedAt;
    public string? Token => _user.GetToken();

    string? IUser.GetToken() => throw new NotSupportedException();
}