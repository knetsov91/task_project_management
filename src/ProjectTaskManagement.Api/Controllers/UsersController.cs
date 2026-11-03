using Microsoft.AspNetCore.Mvc;
using ProjectTaskManagement.Api.DTOs.Users;
using ProjectTaskManagement.Core.Exceptions;
using ProjectTaskManagement.Core.Services;

namespace ProjectTaskManagement.Api.Controllers;

[ApiController]
[Route("api/v1/users")]
public class UsersController(UserService userService) : ControllerBase
{
    [HttpPost]
    [ProducesResponseType<UserResponse>(StatusCodes.Status201Created)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<UserResponse>> CreateAsync(
        CreateUserRequest request, CancellationToken cancellationToken)
    {
        try
        {
            var user = await userService.CreateAsync(
                request.Name, request.Email, cancellationToken);

            var response = new UserResponse
            {
                Id = user.Id,
                Name = user.Name,
                Email = user.Email,
                CreatedAt = user.CreatedAt
            };

            return Created($"{Request.PathBase}/api/v1/users/{user.Id}", response);
        }
        catch (DuplicateEmailException exception)
        {
            return Problem(
                statusCode: StatusCodes.Status409Conflict,
                title: "Email already registered",
                detail: exception.Message);
        }
    }
}
