namespace ProjectTaskManagement.Core.Entities;

public class Project
{
    public const int MaxNameLength = 150;
    public const int MaxDescriptionLength = 2000;

    public Guid Id { get; set; }
    public required string Name { get; set; }
    public string? Description { get; set; }
    public Guid OwnerId { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
}
