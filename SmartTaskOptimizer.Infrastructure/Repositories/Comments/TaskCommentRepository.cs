using Microsoft.EntityFrameworkCore;
using SmartTaskOptimizer.Domain.Entities;
using SmartTaskOptimizer.Domain.Repositories.Comments;
using SmartTaskOptimizer.Infrastructure.Data;

namespace SmartTaskOptimizer.Infrastructure.Repositories.Comments;

public sealed class TaskCommentRepository : ITaskCommentRepository
{
    private readonly AppDbContext _context;
    public TaskCommentRepository(AppDbContext context) => _context = context;

    public async Task AddAsync(TaskComment comment, CancellationToken cancellationToken) =>
        await _context.TaskComments.AddAsync(comment, cancellationToken);

    public Task<List<TaskComment>> GetByTaskIdAsync(Guid taskId, CancellationToken cancellationToken) =>
        _context.TaskComments.AsNoTracking().Include(x => x.Author)
            .Where(x => x.TaskId == taskId && !x.IsDeleted)
            .OrderBy(x => x.CreatedAt).ToListAsync(cancellationToken);

    public Task<TaskComment?> GetByIdAsync(Guid commentId, CancellationToken cancellationToken) =>
        _context.TaskComments.SingleOrDefaultAsync(x => x.Id == commentId && !x.IsDeleted, cancellationToken);

    public Task SaveAsync(CancellationToken cancellationToken) => _context.SaveChangesAsync(cancellationToken);
}
