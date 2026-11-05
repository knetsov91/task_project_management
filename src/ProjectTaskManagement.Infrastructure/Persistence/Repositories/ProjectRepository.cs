using Microsoft.EntityFrameworkCore;
using ProjectTaskManagement.Core.Entities;
using ProjectTaskManagement.Core.Interfaces;

namespace ProjectTaskManagement.Infrastructure.Persistence.Repositories;

public class ProjectRepository(AppDbContext dbContext) : IProjectRepository
{
    public async Task<IReadOnlyList<Project>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await dbContext.Projects.AsNoTracking()
            .OrderBy(project => project.Name)
            .ThenBy(project => project.Id)
            .ToListAsync(cancellationToken);
    }

    public Task<Project?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return dbContext.Projects.AsNoTracking()
            .SingleOrDefaultAsync(project => project.Id == id, cancellationToken);
    }

    public async Task AddAsync(Project project, CancellationToken cancellationToken = default)
    {
        dbContext.Projects.Add(project);
        await dbContext.SaveChangesAsync(cancellationToken);
    }
}
