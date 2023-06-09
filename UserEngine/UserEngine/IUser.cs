﻿namespace UserEngine;

public interface IUser
{
    string Id { get; }
    string Name { get; }
    string Email { get; }
    DateTime CreatedAt { get; }
    DateTime UpdatedAt { get; }
    UserType Type { get; }

    string? GetToken();
}