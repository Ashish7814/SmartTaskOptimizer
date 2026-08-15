using Hangfire;
using Hangfire.SqlServer;
using MediatR;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using SmartTaskOptimizer.API.Middleware;
using SmartTaskOptimizer.API.Health;
using SmartTaskOptimizer.Application.Auth.Service;
using SmartTaskOptimizer.Application.Behaviors;
using SmartTaskOptimizer.Application.Common.Interfaces;
using SmartTaskOptimizer.Application.Priorities;
using SmartTaskOptimizer.Application.Priorities.Strategies;
using SmartTaskOptimizer.Application.Reports.Service;
using SmartTaskOptimizer.Application.Validators;
using SmartTaskOptimizer.Domain.Repositories.Activities;
using SmartTaskOptimizer.Domain.Repositories.Auth;
using SmartTaskOptimizer.Domain.Repositories.BackgroundJobs;
using SmartTaskOptimizer.Domain.Repositories.Comments;
using SmartTaskOptimizer.Domain.Repositories.Dashboard;
using SmartTaskOptimizer.Domain.Repositories.Notifications;
using SmartTaskOptimizer.Domain.Repositories.Project;
using SmartTaskOptimizer.Domain.Repositories.Reports;
using SmartTaskOptimizer.Domain.Repositories.TaskHistoriy;
using SmartTaskOptimizer.Domain.Repositories.Tasks;
using SmartTaskOptimizer.Infrastructure.BackgroundJobs;
using SmartTaskOptimizer.Infrastructure.Data;
using SmartTaskOptimizer.Infrastructure.Hubs;
using SmartTaskOptimizer.Infrastructure.Repositories.Activities;
using SmartTaskOptimizer.Infrastructure.Repositories.Auth;
using SmartTaskOptimizer.Infrastructure.Repositories.Comments;
using SmartTaskOptimizer.Infrastructure.Repositories.Dashboard;
using SmartTaskOptimizer.Infrastructure.Repositories.Notifications;
using SmartTaskOptimizer.Infrastructure.Repositories.Projects;
using SmartTaskOptimizer.Infrastructure.Repositories.Reports;
using SmartTaskOptimizer.Infrastructure.Repositories.TaskHistory;
using SmartTaskOptimizer.Infrastructure.Repositories.Tasks;
using System.Reflection;
using System.Text;
using System.Security.Cryptography;
using Microsoft.OpenApi.Models;
using System.Threading.RateLimiting;
using FluentValidation;
using Microsoft.AspNetCore.RateLimiting;

var builder = WebApplication.CreateBuilder(args);
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
if (string.IsNullOrWhiteSpace(connectionString)) throw new InvalidOperationException("ConnectionStrings:DefaultConnection must be configured.");

builder.Services.AddControllers();
builder.Services.AddResponseCompression(options => options.EnableForHttps = true);
builder.Services.AddProblemDetails();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme { Name = "Authorization", Type = SecuritySchemeType.Http, Scheme = "bearer", BearerFormat = "JWT", In = ParameterLocation.Header });
    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        { new OpenApiSecurityScheme { Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Bearer" } }, Array.Empty<string>() }
    });
});
builder.Services.AddHttpContextAccessor();
builder.Services.AddHealthChecks().AddCheck<DatabaseHealthCheck>("database");

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(connectionString, sql =>
    {
        sql.EnableRetryOnFailure(5, TimeSpan.FromSeconds(10), null);
    }));

builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssemblies(
    Assembly.GetExecutingAssembly(),
    typeof(SmartTaskOptimizer.Application.Auth.Commands.LoginUserCommand).Assembly));
builder.Services.AddValidatorsFromAssemblyContaining<CreateTaskDtoValidator>();
builder.Services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));

var jwtKey = builder.Configuration["Jwt:Key"];
if (string.IsNullOrWhiteSpace(jwtKey) || Encoding.UTF8.GetByteCount(jwtKey) < 32)
{
    if (builder.Environment.IsDevelopment()) jwtKey = Convert.ToBase64String(RandomNumberGenerator.GetBytes(32));
    else throw new InvalidOperationException("Jwt:Key must be supplied through environment configuration and be at least 256 bits.");
}
builder.Configuration["Jwt:Key"] = jwtKey;

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidAudience = builder.Configuration["Jwt:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey)),
            ClockSkew = TimeSpan.FromSeconds(30)
        };
        options.Events = new JwtBearerEvents
        {
            OnMessageReceived = context =>
            {
                var token = context.Request.Query["access_token"];
                if (!string.IsNullOrWhiteSpace(token) && context.HttpContext.Request.Path.StartsWithSegments("/hubs")) context.Token = token;
                return Task.CompletedTask;
            }
        };
    });
builder.Services.AddAuthorization();

builder.Services.AddSignalR(options =>
{
    options.EnableDetailedErrors = builder.Environment.IsDevelopment();
    options.MaximumReceiveMessageSize = 64 * 1024;
});

builder.Services.AddCors(options => options.AddPolicy("Frontend", policy =>
{
    var origins = builder.Configuration.GetSection("Cors:Origins").Get<string[]>() ?? Array.Empty<string>();
    if (origins.Length == 0) throw new InvalidOperationException("Cors:Origins must contain at least one trusted frontend origin.");
    policy.WithOrigins(origins).AllowAnyHeader().AllowAnyMethod().AllowCredentials();
}));

builder.Services.AddRateLimiter(options =>
{
    options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(context => RateLimitPartition.GetFixedWindowLimiter(context.Connection.RemoteIpAddress?.ToString() ?? "unknown", _ => new FixedWindowRateLimiterOptions { PermitLimit = 120, Window = TimeSpan.FromMinutes(1), QueueLimit = 0 }));
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
    options.AddFixedWindowLimiter("auth", limiter =>
    {
        limiter.PermitLimit = 10;
        limiter.Window = TimeSpan.FromMinutes(1);
        limiter.QueueLimit = 0;
    });
    options.AddFixedWindowLimiter("expensive", limiter =>
    {
        limiter.PermitLimit = 20;
        limiter.Window = TimeSpan.FromMinutes(1);
        limiter.QueueLimit = 0;
    });
});

if (builder.Configuration.GetValue("Hangfire:Enabled", true))
{
    builder.Services.AddHangfire(config => config.UseSqlServerStorage(connectionString, new SqlServerStorageOptions { CommandBatchMaxTimeout = TimeSpan.FromMinutes(5), SlidingInvisibilityTimeout = TimeSpan.FromMinutes(5), QueuePollInterval = TimeSpan.FromSeconds(15), UseRecommendedIsolationLevel = true, DisableGlobalLocks = true }));
    builder.Services.AddHangfireServer();
}

builder.Services.AddScoped<IDashboardRepository, DashboardRepository>();
builder.Services.AddScoped<IActivityRepository, ActivityRepository>();
builder.Services.AddScoped<ITaskHistoryRepository, TaskHistoryRepository>();
builder.Services.AddScoped<ITaskRepository, TaskRepository>();
builder.Services.AddScoped<IProjectRepository, ProjectRepository>();
builder.Services.AddScoped<IRealtimeNotifier, RealtimeNotifier>();
builder.Services.AddScoped<ITaskBackgroundJob, TaskBackgroundJob>();
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IJwtTokenService, JwtTokenService>();
builder.Services.AddScoped<IReportRepository, ReportRepository>();
builder.Services.AddScoped<IExportService, ExportService>();
builder.Services.AddScoped<INotificationRepository, NotificationRepository>();
builder.Services.AddScoped<ITaskCommentRepository, TaskCommentRepository>();
builder.Services.AddScoped<ICurrentUserService, SmartTaskOptimizer.Application.Service.Implentation.CurrentUserService>();
builder.Services.AddScoped<IPriorityEngine, PriorityEngine>();
builder.Services.AddScoped<IPriorityStrategy, DeadlinePriorityStrategy>();
builder.Services.AddScoped<IPriorityStrategy, EffortPriorityStrategy>();
builder.Services.AddScoped<IPriorityStrategy, StatusPriorityStrategy>();
builder.Services.AddTransient<ExceptionHandlingMiddleware>();

var app = builder.Build();

if (!app.Environment.IsDevelopment()) app.UseHsts();
app.UseHttpsRedirection();
app.UseResponseCompression();
app.UseCors("Frontend");
app.UseRateLimiter();
app.Use(async (context, next) =>
{
    context.Response.Headers["X-Content-Type-Options"] = "nosniff";
    context.Response.Headers["X-Frame-Options"] = "DENY";
    context.Response.Headers["Referrer-Policy"] = "no-referrer";
    context.Response.Headers["Permissions-Policy"] = "camera=(), microphone=(), geolocation=()";
    await next();
});
app.UseMiddleware<ExceptionHandlingMiddleware>();
app.UseAuthentication();
app.UseAuthorization();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.MapControllers();
app.MapHub<NotificationHub>("/hubs/notifications");
app.MapHealthChecks("/health", new HealthCheckOptions { Predicate = _ => true });

if (builder.Configuration.GetValue("Hangfire:Enabled", true))
    RecurringJob.AddOrUpdate<ITaskBackgroundJob>("recalculate-task-priorities", job => job.RecalculatePrioritiesAsync(), Cron.Hourly);

app.MapHealthChecks("/ready", new HealthCheckOptions { Predicate = check => check.Name == "database" });

app.Run();

public partial class Program { }
