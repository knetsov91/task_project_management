using Microsoft.AspNetCore.Mvc;
using ProjectTaskManagement.Api.DTOs.Projects;
using ProjectTaskManagement.Core.Services;

namespace ProjectTaskManagement.Api.Controllers;

[ApiController]
[Route("api/v1/projects")]
public class ProjectsController(ProjectService projectService) : ControllerBase
{
    [HttpPost]
    [ProducesResponseType<ProjectResponse>(StatusCodes.Status201Created)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ProjectResponse>> CreateAsync(
        CreateProjectRequest request, CancellationToken cancellationToken)
    {
        var project = await projectService.CreateAsync(
            request.Name, request.Description, request.OwnerId, cancellationToken);

        if (project is null)
        {
            return Problem(
                statusCode: StatusCodes.Status404NotFound,
                title: "Project owner not found",
                detail: $"No user with ID '{request.OwnerId}' exists.");
        }

        var response = new ProjectResponse
        {
            Id = project.Id,
            Name = project.Name,
            Description = project.Description,
            OwnerId = project.OwnerId,
            CreatedAt = project.CreatedAt
        };

        return Created($"{Request.PathBase}/api/v1/projects/{project.Id}", response);
    }
}
