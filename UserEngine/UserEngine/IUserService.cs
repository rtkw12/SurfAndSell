using Common.Util;

namespace UserEngine;

public interface IUserService
{
    Task<TryResult<IUser>> TryGetUser(string id);
    Task<TryResult<IUser>> TryAddUser(IUserInput input);
    Task<TryResult<IUser>> TryUpdateUser(string? name = null, string? email = null, string? password = null);
}

public interface IUserInput
{
    string Name { get; }
    string Email { get; }
    string Password { get; }
}

public interface IUser
{
    string Id { get; }
    string Name { get; }
    string Email { get; }

    string GetToken();
}