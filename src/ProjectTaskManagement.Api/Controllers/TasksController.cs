using Microsoft.AspNetCore.Mvc;
using ProjectTaskManagement.Api.DTOs.Tasks;
using ProjectTaskManagement.Core.Entities;
using ProjectTaskManagement.Core.Services;

namespace ProjectTaskManagement.Api.Controllers;

[ApiController]
[Route("api/v1/tasks")]
public class TasksController(TaskService taskService) : ControllerBase
{
    [HttpGet("{id:guid}")]
    [ProducesResponseType<TaskResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<TaskResponse>> GetByIdAsync(
        Guid id, CancellationToken cancellationToken)
    {
        var task = await taskService.GetByIdAsync(id, cancellationToken);

        if (task is null)
        {
            return Problem(statusCode: StatusCodes.Status404NotFound,
                title: "Task not found", detail: $"No task with ID '{id}' exists.");
        }

        return Ok(ToResponse(task));
    }

    private static TaskResponse ToResponse(TaskItem task) => new()
    {
        Id = task.Id,
        ProjectId = task.ProjectId,
        AssignedToId = task.AssignedToId,
        Title = task.Title,
        Description = task.Description,
        Status = task.Status,
        Priority = task.Priority,
        DueDate = task.DueDate,
        CreatedAt = task.CreatedAt
    };
}
