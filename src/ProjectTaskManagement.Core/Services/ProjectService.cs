using ProjectTaskManagement.Core.Entities;
using ProjectTaskManagement.Core.Interfaces;

namespace ProjectTaskManagement.Core.Services;

public class ProjectService(
    IProjectRepository projects,
    IUserRepository users,
    TimeProvider timeProvider)
{
    public Task<IReadOnlyList<Project>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return projects.GetAllAsync(cancellationToken);
    }

    public Task<Project?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return projects.GetByIdAsync(id, cancellationToken);
    }

    public async Task<Project?> CreateAsync(
        string name,
        string? description,
        Guid ownerId,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);

        if (ownerId == Guid.Empty)
        {
            throw new ArgumentException("A project owner is required.", nameof(ownerId));
        }

        if (await users.GetByIdAsync(ownerId, cancellationToken) is null)
        {
            return null;
        }

        var project = new Project
        {
            Id = Guid.NewGuid(),
            Name = name.Trim(),
            Description = string.IsNullOrWhiteSpace(description) ? null : description.Trim(),
            OwnerId = ownerId,
            CreatedAt = timeProvider.GetUtcNow()
        };

        await projects.AddAsync(project, cancellationToken);

        return project;
    }
}
