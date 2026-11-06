using System.ComponentModel.DataAnnotations;
using ProjectTaskManagement.Core.Entities;

namespace ProjectTaskManagement.Api.DTOs.Projects;

public class UpdateProjectRequest : IValidatableObject
{
    [Required]
    [MaxLength(Project.MaxNameLength)]
    public string Name { get; set; } = string.Empty;

    [MaxLength(Project.MaxDescriptionLength)]
    public string? Description { get; set; }

    public Guid OwnerId { get; set; }

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (OwnerId == Guid.Empty)
        {
            yield return new ValidationResult(
                "A project owner is required.", [nameof(OwnerId)]);
        }
    }
}
