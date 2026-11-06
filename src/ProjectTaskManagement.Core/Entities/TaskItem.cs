using ProjectTaskManagement.Core.Enums;

namespace ProjectTaskManagement.Core.Entities;

public class TaskItem
{
    public const int MaxTitleLength = 200;
    public const int MaxDescriptionLength = 4000;

    public Guid Id { get; set; }
    public Guid ProjectId { get; set; }
    public Guid? AssignedToId { get; set; }
    public required string Title { get; set; }
    public string? Description { get; set; }
    public TaskItemStatus Status { get; set; } = TaskItemStatus.Todo;
    public TaskPriority Priority { get; set; } = TaskPriority.Medium;
    public DateTimeOffset? DueDate { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
}
