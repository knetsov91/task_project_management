namespace ProjectTaskManagement.Api.DTOs.Projects;

public class ProjectResponse
{
    public Guid Id { get; init; }
    public required string Name { get; init; }
    public string? Description { get; init; }
    public Guid OwnerId { get; init; }
    public DateTimeOffset CreatedAt { get; init; }
}
