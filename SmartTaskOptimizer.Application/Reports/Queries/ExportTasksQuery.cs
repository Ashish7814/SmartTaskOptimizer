using MediatR;

namespace SmartTaskOptimizer.Application.Reports.Queries;

public sealed record ExportTasksQuery(string Fromat) : IRequest<byte[]>;
