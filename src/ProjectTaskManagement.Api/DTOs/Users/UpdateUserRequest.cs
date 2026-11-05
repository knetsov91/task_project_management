using System.ComponentModel.DataAnnotations;
using ProjectTaskManagement.Core.Entities;

namespace ProjectTaskManagement.Api.DTOs.Users;

public class UpdateUserRequest : IValidatableObject
{
    [MinLength(1)]
    [MaxLength(User.MaxNameLength)]
    public string? Name { get; set; }

    [MinLength(1)]
    [MaxLength(User.MaxEmailLength)]
    [EmailAddress]
    public string? Email { get; set; }

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (Name is null && Email is null)
        {
            yield return new ValidationResult(
                "At least one field must be provided.", [nameof(Name), nameof(Email)]);
        }

        if (Name is not null && string.IsNullOrWhiteSpace(Name))
        {
            yield return new ValidationResult(
                "Name cannot be empty or whitespace.", [nameof(Name)]);
        }

        if (Email is not null && string.IsNullOrWhiteSpace(Email))
        {
            yield return new ValidationResult(
                "Email cannot be empty or whitespace.", [nameof(Email)]);
        }
    }
}
