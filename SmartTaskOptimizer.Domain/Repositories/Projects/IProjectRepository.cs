using SmartTaskOptimizer.Domain.Entities;

namespace SmartTaskOptimizer.Domain.Repositories.Project;

public interface IProjectRepository
{
    Task AddProjectAsync(SmartTaskOptimizer.Domain.Entities.Project project, CancellationToken cancellationToken);
    Task CreateWithOwnerAsync(SmartTaskOptimizer.Domain.Entities.Project project, SmartTaskOptimizer.Domain.Entities.ProjectMember ownerMember, CancellationToken cancellationToken);
    Task UpdateProjectAsync(SmartTaskOptimizer.Domain.Entities.Project project, CancellationToken cancellationToken);
    Task<SmartTaskOptimizer.Domain.Entities.Project?> GetProjectByIdAsync(Guid id, Guid userId, CancellationToken cancellationToken);
    Task<List<SmartTaskOptimizer.Domain.Entities.Project>> GetAllProjectAsync(Guid userId, CancellationToken cancellationToken);
    Task<bool> CanAccessAsync(Guid projectId, Guid userId, CancellationToken cancellationToken);
    Task<bool> IsManagerOrOwnerAsync(Guid projectId, Guid userId, CancellationToken cancellationToken);
    Task AddMemberAsync(ProjectMember member, CancellationToken cancellationToken);
    Task RemoveMemberAsync(Guid projectId, Guid userId, CancellationToken cancellationToken);
    Task SoftDeleteAsync(Guid projectId, Guid userId, CancellationToken cancellationToken);
    Task<List<ProjectMember>> GetMembersAsync(Guid projectId, CancellationToken cancellationToken);
}
