using ProjectTaskManagement.Core.Enums;

namespace ProjectTaskManagement.Api.DTOs.Tasks;

public class TaskResponse
{
    public Guid Id { get; init; }
    public Guid ProjectId { get; init; }
    public Guid? AssignedToId { get; init; }
    public required string Title { get; init; }
    public string? Description { get; init; }
    public TaskItemStatus Status { get; init; }
    public TaskPriority Priority { get; init; }
    public DateTimeOffset? DueDate { get; init; }
    public DateTimeOffset CreatedAt { get; init; }
}
