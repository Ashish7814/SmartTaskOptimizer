using Microsoft.Extensions.Diagnostics.HealthChecks;
using SmartTaskOptimizer.Infrastructure.Data;

namespace SmartTaskOptimizer.API.Health;

public sealed class DatabaseHealthCheck : IHealthCheck
{
    private readonly AppDbContext _db;
    public DatabaseHealthCheck(AppDbContext db) => _db = db;
    public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
    {
        try { return await _db.Database.CanConnectAsync(cancellationToken) ? HealthCheckResult.Healthy() : HealthCheckResult.Unhealthy("Database is unavailable."); }
        catch (Exception ex) { return HealthCheckResult.Unhealthy("Database connectivity check failed.", ex); }
    }
}
