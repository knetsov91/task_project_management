namespace ProjectTaskManagement.Core.Entities;

public sealed class User
{
    public const int MaxNameLength = 100;
    public const int MaxEmailLength = 254;

    public Guid Id { get; set; }
    public required string Name { get; set; }
    public required string Email { get; set; }
    public required string NormalizedEmail { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
}
