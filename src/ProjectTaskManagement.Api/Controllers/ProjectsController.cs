using Microsoft.AspNetCore.Mvc;
using ProjectTaskManagement.Api.DTOs.Projects;
using ProjectTaskManagement.Core.Exceptions;
using ProjectTaskManagement.Core.Services;

namespace ProjectTaskManagement.Api.Controllers;

[ApiController]
[Route("api/v1/projects")]
public class ProjectsController(ProjectService projectService) : ControllerBase
{
    [HttpGet]
    [ProducesResponseType<IReadOnlyList<ProjectResponse>>(StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<ProjectResponse>>> GetAllAsync(
        CancellationToken cancellationToken)
    {
        var projects = await projectService.GetAllAsync(cancellationToken);

        return Ok(projects.Select(project => new ProjectResponse
        {
            Id = project.Id,
            Name = project.Name,
            Description = project.Description,
            OwnerId = project.OwnerId,
            CreatedAt = project.CreatedAt
        }).ToArray());
    }

    [HttpGet("~/api/v1/users/{userId:guid}/projects")]
    [ProducesResponseType<IReadOnlyList<ProjectResponse>>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<IReadOnlyList<ProjectResponse>>> GetByOwnerIdAsync(
        Guid userId, CancellationToken cancellationToken)
    {
        var projects = await projectService.GetByOwnerIdAsync(userId, cancellationToken);

        if (projects is null)
        {
            return Problem(
                statusCode: StatusCodes.Status404NotFound,
                title: "User not found",
                detail: $"No user with ID '{userId}' exists.");
        }

        return Ok(projects.Select(project => new ProjectResponse
        {
            Id = project.Id,
            Name = project.Name,
            Description = project.Description,
            OwnerId = project.OwnerId,
            CreatedAt = project.CreatedAt
        }).ToArray());
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType<ProjectResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ProjectResponse>> GetByIdAsync(
        Guid id, CancellationToken cancellationToken)
    {
        var project = await projectService.GetByIdAsync(id, cancellationToken);

        if (project is null)
        {
            return Problem(
                statusCode: StatusCodes.Status404NotFound,
                title: "Project not found",
                detail: $"No project with ID '{id}' exists.");
        }

        return Ok(new ProjectResponse
        {
            Id = project.Id,
            Name = project.Name,
            Description = project.Description,
            OwnerId = project.OwnerId,
            CreatedAt = project.CreatedAt
        });
    }

    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteAsync(Guid id, CancellationToken cancellationToken)
    {
        if (!await projectService.DeleteAsync(id, cancellationToken))
        {
            return Problem(
                statusCode: StatusCodes.Status404NotFound,
                title: "Project not found",
                detail: $"No project with ID '{id}' exists.");
        }

        return NoContent();
    }

    [HttpPut("{id:guid}")]
    [ProducesResponseType<ProjectResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ProjectResponse>> UpdateAsync(
        Guid id, UpdateProjectRequest request, CancellationToken cancellationToken)
    {
        try
        {
            var project = await projectService.UpdateAsync(
                id, request.Name, request.Description, request.OwnerId, cancellationToken);

            if (project is null)
            {
                return Problem(
                    statusCode: StatusCodes.Status404NotFound,
                    title: "Project not found",
                    detail: $"No project with ID '{id}' exists.");
            }

            return Ok(new ProjectResponse
            {
                Id = project.Id,
                Name = project.Name,
                Description = project.Description,
                OwnerId = project.OwnerId,
                CreatedAt = project.CreatedAt
            });
        }
        catch (UserNotFoundException exception)
        {
            return Problem(
                statusCode: StatusCodes.Status404NotFound,
                title: "Project owner not found",
                detail: exception.Message);
        }
    }

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
