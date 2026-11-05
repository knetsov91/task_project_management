using System.ComponentModel.DataAnnotations;
using ProjectTaskManagement.Core.Entities;

namespace ProjectTaskManagement.Api.DTOs.Users;

public class CreateUserRequest
{
    [Required]
    [MaxLength(User.MaxNameLength)]
    public string Name { get; set; } = string.Empty;

    [Required]
    [MaxLength(User.MaxEmailLength)]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;
}
