using Microsoft.EntityFrameworkCore;
using SmartTaskOptimizer.Domain.Entities;
using SmartTaskOptimizer.Domain.Repositories.Project;
using SmartTaskOptimizer.Infrastructure.Data;

namespace SmartTaskOptimizer.Infrastructure.Repositories.Projects;

public sealed class ProjectRepository : IProjectRepository
{
    private readonly AppDbContext _context;
    public ProjectRepository(AppDbContext context) => _context = context;

    public async Task AddProjectAsync(Project project, CancellationToken cancellationToken)
    {
        await _context.Projects.AddAsync(project, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task CreateWithOwnerAsync(Project project, ProjectMember ownerMember, CancellationToken cancellationToken)
    {
        await using var transaction = await _context.Database.BeginTransactionAsync(cancellationToken);
        await _context.Projects.AddAsync(project, cancellationToken);
        await _context.ProjectMembers.AddAsync(ownerMember, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);
    }

    public async Task UpdateProjectAsync(Project project, CancellationToken cancellationToken)
    {
        await _context.Projects.Where(x => x.Id == project.Id).ExecuteUpdateAsync(setters => setters
            .SetProperty(x => x.Name, project.Name)
            .SetProperty(x => x.Description, project.Description)
            .SetProperty(x => x.UpdatedAt, DateTime.UtcNow), cancellationToken);
    }

    public Task<Project?> GetProjectByIdAsync(Guid id, Guid userId, CancellationToken cancellationToken) =>
        _context.Projects.AsNoTracking()
            .Include(p => p.Members).ThenInclude(m => m.User)
            .Include(p => p.Tasks)
            .SingleOrDefaultAsync(p => p.Id == id && (_context.Users.Any(u => u.Id == userId && u.Role == SmartTaskOptimizer.Shared.Enums.UserRole.Admin) || p.OwnerId == userId || p.Members.Any(m => m.UserId == userId)), cancellationToken);

    public Task<List<Project>> GetAllProjectAsync(Guid userId, CancellationToken cancellationToken) =>
        _context.Projects.AsNoTracking()
            .Include(p => p.Members)
            .Include(p => p.Tasks)
            .Where(p => _context.Users.Any(u => u.Id == userId && u.Role == SmartTaskOptimizer.Shared.Enums.UserRole.Admin) || p.OwnerId == userId || p.Members.Any(m => m.UserId == userId))
            .OrderBy(p => p.Name).ToListAsync(cancellationToken);

    public Task<bool> CanAccessAsync(Guid projectId, Guid userId, CancellationToken cancellationToken) =>
        _context.Projects.AnyAsync(p => p.Id == projectId && (_context.Users.Any(u => u.Id == userId && u.Role == SmartTaskOptimizer.Shared.Enums.UserRole.Admin) || p.OwnerId == userId || p.Members.Any(m => m.UserId == userId)), cancellationToken);

    public Task<bool> IsManagerOrOwnerAsync(Guid projectId, Guid userId, CancellationToken cancellationToken) =>
        _context.Projects.AnyAsync(p => p.Id == projectId && (_context.Users.Any(u => u.Id == userId && u.Role == SmartTaskOptimizer.Shared.Enums.UserRole.Admin) || p.OwnerId == userId || p.Members.Any(m => m.UserId == userId && (m.Role == "Owner" || m.Role == "Manager"))), cancellationToken);

    public async Task AddMemberAsync(ProjectMember member, CancellationToken cancellationToken)
    {
        var exists = await _context.ProjectMembers.AnyAsync(x => x.ProjectId == member.ProjectId && x.UserId == member.UserId, cancellationToken);
        if (exists) return;
        _context.ProjectMembers.Add(member);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task SoftDeleteAsync(Guid projectId, Guid userId, CancellationToken cancellationToken)
    {
        await _context.Projects.Where(x => x.Id == projectId && x.OwnerId == userId).ExecuteUpdateAsync(setters => setters.SetProperty(x => x.IsDeleted, true).SetProperty(x => x.DeletedAt, DateTime.UtcNow), cancellationToken);
    }

    public async Task RemoveMemberAsync(Guid projectId, Guid userId, CancellationToken cancellationToken)
    {
        var member = await _context.ProjectMembers.SingleOrDefaultAsync(x => x.ProjectId == projectId && x.UserId == userId, cancellationToken);
        if (member is null) return;
        _context.ProjectMembers.Remove(member);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public Task<List<ProjectMember>> GetMembersAsync(Guid projectId, CancellationToken cancellationToken) =>
        _context.ProjectMembers.AsNoTracking().Include(x => x.User).Where(x => x.ProjectId == projectId).OrderBy(x => x.User.FullName).ToListAsync(cancellationToken);
}
