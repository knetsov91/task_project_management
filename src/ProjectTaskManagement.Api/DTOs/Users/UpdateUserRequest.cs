using System.ComponentModel.DataAnnotations;
using ProjectTaskManagement.Core.Entities;

namespace ProjectTaskManagement.Api.DTOs.Users;

public class UpdateUserRequest
{
    [MinLength(1)]
    [MaxLength(User.MaxNameLength)]
    public string? Name { get; set; }

    [MinLength(1)]
    [MaxLength(User.MaxEmailLength)]
    [EmailAddress]
    public string? Email { get; set; }
}
