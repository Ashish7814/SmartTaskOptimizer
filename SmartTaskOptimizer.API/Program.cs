using Hangfire;
using Hangfire.SqlServer;
using MediatR;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using SmartTaskOptimizer.API.Health;
using SmartTaskOptimizer.API.Middleware;
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
using System.Security.Cryptography;
using System.Text;
using System.Threading.RateLimiting;
using FluentValidation;

var builder = WebApplication.CreateBuilder(args);

// ============================================================
// Configuration
// ============================================================

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

if (string.IsNullOrWhiteSpace(connectionString))
{
    throw new InvalidOperationException(
        "ConnectionStrings:DefaultConnection must be configured.");
}

// ============================================================
// Controllers / API
// ============================================================

builder.Services.AddControllers();

builder.Services.AddEndpointsApiExplorer();

builder.Services.AddProblemDetails();

builder.Services.AddHttpContextAccessor();

// ============================================================
// CSRF
// ============================================================

builder.Services.AddSingleton<CsrfTokenService>();
// ============================================================
// Response Compression
// ============================================================

builder.Services.AddResponseCompression(options =>
{
    options.EnableForHttps = true;
});

// ============================================================
// Swagger
// ============================================================

builder.Services.AddSwaggerGen(options =>
{
    options.AddSecurityDefinition(
        "Bearer",
        new OpenApiSecurityScheme
        {
            Name = "Authorization",
            Type = SecuritySchemeType.Http,
            Scheme = "bearer",
            BearerFormat = "JWT",
            In = ParameterLocation.Header,
            Description = "Enter JWT token."
        });

    options.AddSecurityRequirement(
        new OpenApiSecurityRequirement
        {
            {
                new OpenApiSecurityScheme
                {
                    Reference = new OpenApiReference
                    {
                        Type = ReferenceType.SecurityScheme,
                        Id = "Bearer"
                    }
                },
                Array.Empty<string>()
            }
        });
});

// ============================================================
// Health Checks
// ============================================================

builder.Services
    .AddHealthChecks()
    .AddCheck<DatabaseHealthCheck>("database");

// ============================================================
// Entity Framework Core
// ============================================================

builder.Services.AddDbContext<AppDbContext>(options =>
{
    options.UseSqlServer(
        connectionString,
        sqlOptions =>
        {
            sqlOptions.EnableRetryOnFailure(
                maxRetryCount: 5,
                maxRetryDelay: TimeSpan.FromSeconds(10),
                errorNumbersToAdd: null);
        });
});

// ============================================================
// MediatR
// ============================================================

builder.Services.AddMediatR(configuration =>
{
    configuration.RegisterServicesFromAssemblies(
        Assembly.GetExecutingAssembly(),
        typeof(
            SmartTaskOptimizer.Application.Auth.Commands.LoginUserCommand)
            .Assembly);
});

// ============================================================
// FluentValidation
// ============================================================

builder.Services.AddValidatorsFromAssemblyContaining<CreateTaskDtoValidator>();

builder.Services.AddTransient(
    typeof(IPipelineBehavior<,>),
    typeof(ValidationBehavior<,>));

// ============================================================
// JWT Authentication
// ============================================================

var jwtKey = builder.Configuration["Jwt:Key"];

if (string.IsNullOrWhiteSpace(jwtKey) ||
    Encoding.UTF8.GetByteCount(jwtKey) < 32)
{
    if (builder.Environment.IsDevelopment())
    {
        jwtKey = Convert.ToBase64String(
            RandomNumberGenerator.GetBytes(32));
    }
    else
    {
        throw new InvalidOperationException(
            "Jwt:Key must be supplied through environment configuration " +
            "and be at least 256 bits.");
    }
}

builder.Services
    .AddAuthentication(
        JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters =
            new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,

                ValidIssuer =
                    builder.Configuration["Jwt:Issuer"],

                ValidAudience = builder.Configuration["Jwt:Audience"],

                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey)),

                ClockSkew = TimeSpan.FromSeconds(30)
            };

        options.Events = new JwtBearerEvents
        {
            OnMessageReceived = context =>
            {
                var accessToken =
                    context.Request.Query["access_token"];

                if (!string.IsNullOrWhiteSpace(
                        accessToken) &&
                    context.HttpContext.Request.Path
                        .StartsWithSegments("/hubs"))
                {
                    context.Token = accessToken;
                }

                return Task.CompletedTask;
            }
        };
    });

builder.Services.AddAuthorization();

// ============================================================
// SignalR
// ============================================================

builder.Services.AddSignalR(options =>
{
    options.EnableDetailedErrors =
        builder.Environment.IsDevelopment();

    options.MaximumReceiveMessageSize = 64 * 1024;
});

// ============================================================
// CORS
// ============================================================

builder.Services.AddCors(options =>
{
    options.AddPolicy("Frontend", policy =>
    {
        var origins =
            builder.Configuration
                .GetSection("Cors:Origins")
                .Get<string[]>();

        if (origins == null || origins.Length == 0)
        {
            throw new InvalidOperationException(
                "Cors:Origins must contain at least one trusted frontend origin.");
        }

        policy
            .WithOrigins(origins)
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials();
    });
});

// ============================================================
// Rate Limiting
// ============================================================

builder.Services.AddRateLimiter(options =>
{
    options.GlobalLimiter =
        PartitionedRateLimiter.Create<HttpContext, string>(
            context =>
                RateLimitPartition.GetFixedWindowLimiter(
                    context.Connection.RemoteIpAddress?.ToString()
                        ?? "unknown",
                    _ => new FixedWindowRateLimiterOptions
                    {
                        PermitLimit = 120,
                        Window = TimeSpan.FromMinutes(1),
                        QueueLimit = 0
                    }));

    options.RejectionStatusCode =
        StatusCodes.Status429TooManyRequests;

    options.AddFixedWindowLimiter(
        "auth",
        limiter =>
        {
            limiter.PermitLimit = 10;
            limiter.Window = TimeSpan.FromMinutes(1);
            limiter.QueueLimit = 0;
        });

    options.AddFixedWindowLimiter(
        "expensive",
        limiter =>
        {
            limiter.PermitLimit = 20;
            limiter.Window = TimeSpan.FromMinutes(1);
            limiter.QueueLimit = 0;
        });
});

// ============================================================
// Hangfire
// ============================================================

var hangfireEnabled =
    builder.Configuration.GetValue(
        "Hangfire:Enabled",
        true);

if (hangfireEnabled)
{
    builder.Services.AddHangfire(configuration =>
    {
        configuration
            .UseSimpleAssemblyNameTypeSerializer()
            .UseRecommendedSerializerSettings()
            .UseSqlServerStorage(
                connectionString,
                new SqlServerStorageOptions
                {
                    CommandBatchMaxTimeout =
                        TimeSpan.FromMinutes(5),

                    SlidingInvisibilityTimeout =
                        TimeSpan.FromMinutes(5),

                    QueuePollInterval =
                        TimeSpan.FromSeconds(15),

                    UseRecommendedIsolationLevel = true,

                    DisableGlobalLocks = true
                });
    });

    builder.Services.AddHangfireServer();
}

// ============================================================
// Dependency Injection - Repositories
// ============================================================

builder.Services.AddScoped<IDashboardRepository, DashboardRepository>();

builder.Services.AddScoped<IActivityRepository, ActivityRepository>();

builder.Services.AddScoped<ITaskHistoryRepository, TaskHistoryRepository>();

builder.Services.AddScoped<ITaskRepository, TaskRepository>();

builder.Services.AddScoped<IProjectRepository, ProjectRepository>();

builder.Services.AddScoped<IUserRepository, UserRepository>();

builder.Services.AddScoped<IReportRepository, ReportRepository>();

builder.Services.AddScoped<INotificationRepository, NotificationRepository>();

builder.Services.AddScoped<ITaskCommentRepository, TaskCommentRepository>();

// ============================================================
// Dependency Injection - Services
// ============================================================

builder.Services.AddScoped<IRealtimeNotifier, RealtimeNotifier>();

builder.Services.AddScoped<ITaskBackgroundJob, TaskBackgroundJob>();

builder.Services.AddScoped<IJwtTokenService, JwtTokenService>();
builder.Services.AddScoped<IRefreshTokenRepository, RefreshTokenRepository>();
builder.Services.AddSingleton<RefreshTokenService>();

builder.Services.AddScoped<IExportService, ExportService>();

builder.Services.AddScoped<ICurrentUserService,
    SmartTaskOptimizer.Application.Service.Implentation.CurrentUserService>();

// ============================================================
// Priority Engine
// ============================================================

builder.Services.AddScoped<IPriorityEngine, PriorityEngine>();

builder.Services.AddScoped<IPriorityStrategy, DeadlinePriorityStrategy>();

builder.Services.AddScoped<IPriorityStrategy, EffortPriorityStrategy>();

builder.Services.AddScoped<IPriorityStrategy, StatusPriorityStrategy>();

// ============================================================
// Middleware
// ============================================================

builder.Services.AddTransient<ExceptionHandlingMiddleware>();

// ============================================================
// Build Application
// ============================================================

var app = builder.Build();

// ============================================================
// Production Security
// ============================================================

if (!app.Environment.IsDevelopment())
{
    app.UseHsts();
}

// ============================================================
// Middleware Pipeline
// ============================================================

app.UseHttpsRedirection();

app.UseResponseCompression();

app.UseCors("Frontend");

app.UseRateLimiter();

// ============================================================
// Security Headers
// ============================================================

app.Use(async (context, next) =>
{
    context.Response.Headers["X-Content-Type-Options"] =
        "nosniff";

    context.Response.Headers["X-Frame-Options"] =
        "DENY";

    context.Response.Headers["Referrer-Policy"] =
        "no-referrer";

    context.Response.Headers["Permissions-Policy"] =
        "camera=(), microphone=(), geolocation=()";

    await next();
});

// ============================================================
// Exception Handling
// ============================================================

app.UseMiddleware<ExceptionHandlingMiddleware>();

// ============================================================
// Authentication / Authorization
// ============================================================

app.UseAuthentication();

app.UseAuthorization();

// ============================================================
// Swagger - Development Only
// ============================================================

app.UseSwagger();

app.UseSwaggerUI(options =>
{
    options.SwaggerEndpoint("/swagger/v1/swagger.json", "SmartTaskOptimizer API v1");
    options.RoutePrefix = "swagger";
});

// ============================================================
// API Controllers
// ============================================================

app.MapControllers();

// ============================================================
// SignalR
// ============================================================

app.MapHub<NotificationHub>(
    "/hubs/notifications");

// ============================================================
// Health Checks
// ============================================================

app.MapHealthChecks(
    "/health",
    new HealthCheckOptions
    {
        Predicate = _ => true
    });

app.MapHealthChecks(
    "/ready",
    new HealthCheckOptions
    {
        Predicate = check =>
            check.Name == "database"
    });

// ============================================================
// Hangfire Recurring Jobs
// ============================================================
//
// IMPORTANT:
// Do NOT use:
//
// RecurringJob.AddOrUpdate(...)
//
// That uses Hangfire.JobStorage.Current and was causing
// the MonsterASP.NET startup crash.
//
// Instead, resolve IRecurringJobManager from DI.
// ============================================================

if (hangfireEnabled)
{
    using var scope = app.Services.CreateScope();

    var recurringJobManager =
        scope.ServiceProvider
            .GetRequiredService<IRecurringJobManager>();

    recurringJobManager.AddOrUpdate<ITaskBackgroundJob>(
        "recalculate-task-priorities",
        job => job.RecalculatePrioritiesAsync(),
        Cron.Hourly);
}

// ============================================================
// Run Application
// ============================================================

app.Run();

// Required for integration testing
public partial class Program
{
}
