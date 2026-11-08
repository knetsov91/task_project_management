using System.ComponentModel.DataAnnotations;
using ProjectTaskManagement.Core.Entities;
using ProjectTaskManagement.Core.Enums;

namespace ProjectTaskManagement.Api.DTOs.Tasks;

public class CreateTaskRequest : IValidatableObject
{
    [Required]
    [MaxLength(TaskItem.MaxTitleLength)]
    public string Title { get; set; } = string.Empty;

    [MaxLength(TaskItem.MaxDescriptionLength)]
    public string? Description { get; set; }

    [EnumDataType(typeof(TaskPriority))]
    public TaskPriority Priority { get; set; } = TaskPriority.Medium;

    public DateTimeOffset? DueDate { get; set; }
    public Guid? AssignedToId { get; set; }

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (AssignedToId == Guid.Empty)
        {
            yield return new ValidationResult(
                "An assignee ID cannot be empty.", [nameof(AssignedToId)]);
        }
    }
}
