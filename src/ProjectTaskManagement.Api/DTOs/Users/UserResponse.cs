namespace ProjectTaskManagement.Api.DTOs.Users;

public class UserResponse
{
    public Guid Id { get; init; }
    public required string Name { get; init; }
    public required string Email { get; init; }
    public DateTimeOffset CreatedAt { get; init; }
}
