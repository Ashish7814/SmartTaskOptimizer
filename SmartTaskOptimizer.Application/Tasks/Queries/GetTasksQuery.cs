using MediatR;
using SmartTaskOptimizer.Shared.DTOs.Common;
using SmartTaskOptimizer.Shared.DTOs.Tasks;

namespace SmartTaskOptimizer.Application.Tasks.Queries;

public sealed record GetTasksQuery(TaskQueryDto Query) : IRequest<PagedResult<TaskDto>>;
