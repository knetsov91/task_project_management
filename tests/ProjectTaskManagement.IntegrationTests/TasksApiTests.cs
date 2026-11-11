using System.Net;
using System.Net.Http.Json;
using System.Text;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Moq;
using ProjectTaskManagement.Api.DTOs.Tasks;
using ProjectTaskManagement.Core.Entities;
using ProjectTaskManagement.Core.Enums;
using ProjectTaskManagement.Core.Interfaces;

namespace ProjectTaskManagement.IntegrationTests;

public class TasksApiTests
{
    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task CreateTask_ReturnsCreatedResponseAndRetrievableLocation(bool assignUser)
    {
        using var factory = new TaskApiFactory();
        using var client = factory.CreateClient();
        var projectId = Guid.NewGuid();
        Guid? assigneeId = assignUser ? Guid.NewGuid() : null;
        factory.Projects.Setup(repository => repository.GetByIdAsync(projectId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Project { Id = projectId, Name = "Portfolio", OwnerId = Guid.NewGuid() });
        if (assigneeId is Guid userId)
        {
            factory.Users.Setup(repository => repository.GetByIdAsync(userId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new User
                {
                    Id = userId, Name = "Alice", Email = "alice@example.com",
                    NormalizedEmail = "ALICE@EXAMPLE.COM"
                });
        }

        TaskItem? saved = null;
        factory.Tasks.Setup(repository => repository.AddAsync(It.IsAny<TaskItem>(), It.IsAny<CancellationToken>()))
            .Callback<TaskItem, CancellationToken>((task, _) => saved = task)
            .Returns(Task.CompletedTask);
        factory.Tasks.Setup(repository => repository.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Guid id, CancellationToken _) => saved?.Id == id ? saved : null);
        var dueDate = new DateTimeOffset(2026, 12, 1, 12, 0, 0, TimeSpan.FromHours(2));

        using var response = await client.PostAsJsonAsync($"/api/v1/projects/{projectId}/tasks",
            new CreateTaskRequest
            {
                Title = " Implement API ", Description = " Endpoints ",
                DueDate = dueDate, AssignedToId = assigneeId
            });

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var created = await response.Content.ReadFromJsonAsync<TaskResponse>();
        Assert.NotNull(created);
        Assert.NotEqual(Guid.Empty, created.Id);
        Assert.Equal(projectId, created.ProjectId);
        Assert.Equal(assigneeId, created.AssignedToId);
        Assert.Equal("Implement API", created.Title);
        Assert.Equal("Endpoints", created.Description);
        Assert.Equal(TaskItemStatus.Todo, created.Status);
        Assert.Equal(TaskPriority.Medium, created.Priority);
        Assert.Equal(dueDate.ToUniversalTime(), created.DueDate);
        Assert.Equal(TimeSpan.Zero, created.DueDate!.Value.Offset);
        Assert.Equal(TaskApiFactory.Now, created.CreatedAt);
        Assert.Equal($"/api/v1/tasks/{created.Id}", response.Headers.Location?.OriginalString);

        using var getResponse = await client.GetAsync(response.Headers.Location);
        Assert.Equal(HttpStatusCode.OK, getResponse.StatusCode);
        var retrieved = await getResponse.Content.ReadFromJsonAsync<TaskResponse>();
        Assert.NotNull(retrieved);
        Assert.Equal(created.Id, retrieved.Id);
        Assert.Equal(created.Title, retrieved.Title);
        factory.Tasks.Verify(repository => repository.AddAsync(It.IsAny<TaskItem>(),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Theory]
    [InlineData("{}")]
    [InlineData("{\"title\":null}")]
    [InlineData("{\"title\":\"   \"}")]
    [InlineData("{\"title\":\"Task\",\"priority\":99}")]
    [InlineData("{\"title\":\"Task\",\"priority\":null}")]
    [InlineData("{\"title\":\"Task\",\"assignedToId\":\"00000000-0000-0000-0000-000000000000\"}")]
    [InlineData("{\"title\":\"Task\",\"dueDate\":\"invalid\"}")]
    public async Task CreateTask_WithInvalidInput_ReturnsValidationProblemBeforePersistence(string json)
    {
        using var factory = new TaskApiFactory();
        using var client = factory.CreateClient();
        using var content = new StringContent(json, Encoding.UTF8, "application/json");

        using var response = await client.PostAsync($"/api/v1/projects/{Guid.NewGuid()}/tasks", content);

        var problem = await AssertProblemAsync(response, HttpStatusCode.BadRequest);
        Assert.NotEmpty(problem.Errors);
        factory.Projects.VerifyNoOtherCalls();
        factory.Users.VerifyNoOtherCalls();
        factory.Tasks.VerifyNoOtherCalls();
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task CreateTask_WithOverlongText_ReturnsBadRequest(bool overlongTitle)
    {
        using var factory = new TaskApiFactory();
        using var client = factory.CreateClient();
        using var response = await client.PostAsJsonAsync($"/api/v1/projects/{Guid.NewGuid()}/tasks",
            new CreateTaskRequest
            {
                Title = overlongTitle ? new string('a', TaskItem.MaxTitleLength + 1) : "Task",
                Description = overlongTitle ? null : new string('a', TaskItem.MaxDescriptionLength + 1)
            });

        await AssertProblemAsync(response, HttpStatusCode.BadRequest);
        factory.Projects.VerifyNoOtherCalls();
        factory.Tasks.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task CreateTask_WithEmptyProjectId_ReturnsBadRequest()
    {
        using var factory = new TaskApiFactory();
        using var client = factory.CreateClient();
        using var response = await client.PostAsJsonAsync($"/api/v1/projects/{Guid.Empty}/tasks",
            new CreateTaskRequest { Title = "Task" });

        await AssertProblemAsync(response, HttpStatusCode.BadRequest);
        factory.Projects.VerifyNoOtherCalls();
        factory.Tasks.VerifyNoOtherCalls();
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task CreateTask_WithMissingProjectOrAssignee_ReturnsNotFoundWithoutSaving(bool missingAssignee)
    {
        using var factory = new TaskApiFactory();
        using var client = factory.CreateClient();
        var projectId = Guid.NewGuid();
        var assigneeId = Guid.NewGuid();
        factory.Projects.Setup(repository => repository.GetByIdAsync(projectId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(missingAssignee
                ? new Project { Id = projectId, Name = "Portfolio", OwnerId = Guid.NewGuid() }
                : null);
        if (missingAssignee)
        {
            factory.Users.Setup(repository => repository.GetByIdAsync(assigneeId, It.IsAny<CancellationToken>()))
                .ReturnsAsync((User?)null);
        }

        using var response = await client.PostAsJsonAsync($"/api/v1/projects/{projectId}/tasks",
            new CreateTaskRequest { Title = "Task", AssignedToId = assigneeId });

        var problem = await AssertProblemAsync(response, HttpStatusCode.NotFound);
        Assert.Equal(missingAssignee ? "Task assignee not found" : "Project not found", problem?.Title);
        factory.Tasks.VerifyNoOtherCalls();
        if (!missingAssignee)
        {
            factory.Users.VerifyNoOtherCalls();
        }
    }

    [Fact]
    public async Task GetTask_WhenMissing_ReturnsNotFoundProblem()
    {
        using var factory = new TaskApiFactory();
        using var client = factory.CreateClient();
        var id = Guid.NewGuid();
        factory.Tasks.Setup(repository => repository.GetByIdAsync(id, It.IsAny<CancellationToken>()))
            .ReturnsAsync((TaskItem?)null);

        using var response = await client.GetAsync($"/api/v1/tasks/{id}");

        var problem = await AssertProblemAsync(response, HttpStatusCode.NotFound);
        Assert.Equal("Task not found", problem?.Title);
    }

    private static async Task<ValidationProblemDetails> AssertProblemAsync(
        HttpResponseMessage response, HttpStatusCode status)
    {
        Assert.Equal(status, response.StatusCode);
        Assert.Equal("application/problem+json", response.Content.Headers.ContentType?.MediaType);
        var problem = await response.Content.ReadFromJsonAsync<ValidationProblemDetails>();
        Assert.NotNull(problem);
        Assert.Equal((int)status, problem.Status);
        return problem;
    }

    private class TaskApiFactory : WebApplicationFactory<Program>
    {
        public static readonly DateTimeOffset Now = new(2026, 11, 8, 10, 0, 0, TimeSpan.Zero);
        public Mock<ITaskRepository> Tasks { get; } = new(MockBehavior.Strict);
        public Mock<IProjectRepository> Projects { get; } = new(MockBehavior.Strict);
        public Mock<IUserRepository> Users { get; } = new(MockBehavior.Strict);

        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.UseSetting("ConnectionStrings:DefaultConnection",
                "Server=localhost;Database=Unused;Integrated Security=true;TrustServerCertificate=true");
            builder.ConfigureServices(services =>
            {
                services.RemoveAll<ITaskRepository>();
                services.RemoveAll<IProjectRepository>();
                services.RemoveAll<IUserRepository>();
                services.RemoveAll<TimeProvider>();
                services.AddSingleton(Tasks.Object);
                services.AddSingleton(Projects.Object);
                services.AddSingleton(Users.Object);
                var clock = new Mock<TimeProvider>();
                clock.Setup(provider => provider.GetUtcNow()).Returns(Now);
                services.AddSingleton(clock.Object);
            });
        }
    }
}
