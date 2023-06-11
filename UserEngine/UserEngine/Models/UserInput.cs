namespace UserEngine.Models;

public class UserInput : IUserInput
{
    public UserInput() { }

    public string Name { get; set; }
    public string Email { get; set; }
    public string Password { get; set; }
}