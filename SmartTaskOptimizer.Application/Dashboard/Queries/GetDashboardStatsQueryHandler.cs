using MediatR;
using SmartTaskOptimizer.Domain.Repositories.Dashboard;
using SmartTaskOptimizer.Application.Common.Interfaces;
using SmartTaskOptimizer.Shared.DTOs.Dashboard;

namespace SmartTaskOptimizer.Application.Dashboard.Queries;

public sealed class GetDashboardStatsQueryHandler : IRequestHandler<GetDashboardStatsQuery, DashboardStatsDto>
{
    private readonly IDashboardRepository _repository;
    private readonly ICurrentUserService _currentUser;
    public GetDashboardStatsQueryHandler(IDashboardRepository repository, ICurrentUserService currentUser) { _repository = repository; _currentUser = currentUser; }
    public Task<DashboardStatsDto> Handle(GetDashboardStatsQuery request, CancellationToken cancellationToken) => _repository.GetDashboardStatsAsync(_currentUser.UserId, cancellationToken);
}
