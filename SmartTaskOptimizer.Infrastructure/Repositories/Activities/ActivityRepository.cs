using Microsoft.EntityFrameworkCore;
using SmartTaskOptimizer.Domain.Entities;
using SmartTaskOptimizer.Domain.Repositories.Activities;
using SmartTaskOptimizer.Infrastructure.Data;

namespace SmartTaskOptimizer.Infrastructure.Repositories.Activities;

public sealed class ActivityRepository : IActivityRepository
{
    private readonly AppDbContext _context;
    public ActivityRepository(AppDbContext context) => _context = context;
    public async Task AddAsync(Activity activity, CancellationToken cancellationToken) { await _context.Activities.AddAsync(activity, cancellationToken); await _context.SaveChangesAsync(cancellationToken); }
    public Task<List<Activity>> GetByProjectAsync(Guid projectId, int page, int pageSize, CancellationToken cancellationToken) =>
        _context.Activities.AsNoTracking().Include(x => x.Actor).Include(x => x.Task).Where(x => x.ProjectId == projectId).OrderByDescending(x => x.CreatedAt).Skip((Math.Max(1, page) - 1) * Math.Clamp(pageSize, 1, 100)).Take(Math.Clamp(pageSize, 1, 100)).ToListAsync(cancellationToken);
}
