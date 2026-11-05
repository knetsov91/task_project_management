using ProjectTaskManagement.Core.Entities;
using ProjectTaskManagement.Core.Interfaces;

namespace ProjectTaskManagement.Infrastructure.Persistence.Repositories;

public class ProjectRepository(AppDbContext dbContext) : IProjectRepository
{
    public async Task AddAsync(Project project, CancellationToken cancellationToken = default)
    {
        dbContext.Projects.Add(project);
        await dbContext.SaveChangesAsync(cancellationToken);
    }
}
