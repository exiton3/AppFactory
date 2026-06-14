namespace AspNetCore.UserService.Application.Commands;

/// <summary>
/// HTTP request wrapper for CreateUserCommand
/// </summary>
public class CreateUserRequest
{
    /// <summary>
    /// The command to execute
    /// </summary>
    public CreateUserCommand Command { get; set; } = new();
}
