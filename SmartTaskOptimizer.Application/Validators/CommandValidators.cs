using FluentValidation;
using SmartTaskOptimizer.Application.Auth.Commands;
using SmartTaskOptimizer.Application.Project.Commands;
using SmartTaskOptimizer.Application.Tasks.Commands.Create;
using SmartTaskOptimizer.Application.Tasks.Commands.Update;
using SmartTaskOptimizer.Shared.Enums;

namespace SmartTaskOptimizer.Application.Validators;

public sealed class RegisterUserCommandValidator : AbstractValidator<RegisterUserCommand>
{
    public RegisterUserCommandValidator()
    {
        RuleFor(x => x.dto.FullName).NotEmpty().MaximumLength(150);
        RuleFor(x => x.dto.Email).NotEmpty().EmailAddress().MaximumLength(320);
        RuleFor(x => x.dto.Password).NotEmpty().MinimumLength(8).MaximumLength(128)
            .Matches("[A-Z]").WithMessage("Password must contain an uppercase letter.")
            .Matches("[a-z]").WithMessage("Password must contain a lowercase letter.")
            .Matches("[0-9]").WithMessage("Password must contain a number.");
    }
}

public sealed class LoginUserCommandValidator : AbstractValidator<LoginUserCommand>
{
    public LoginUserCommandValidator() { RuleFor(x => x.Dto.Email).NotEmpty().EmailAddress(); RuleFor(x => x.Dto.Password).NotEmpty(); }
}

public sealed class CreateProjectCommandValidator : AbstractValidator<CreateProjectCommand>
{
    public CreateProjectCommandValidator() { RuleFor(x => x.dto.Name).NotEmpty().MaximumLength(200); RuleFor(x => x.dto.Description).MaximumLength(2000); }
}

public sealed class UpdateProjectCommandValidator : AbstractValidator<UpdateProjectCommand>
{
    public UpdateProjectCommandValidator() { RuleFor(x => x.dto.Name).NotEmpty().MaximumLength(200); RuleFor(x => x.dto.Description).MaximumLength(2000); }
}

public sealed class CreateTaskCommandValidator : AbstractValidator<CreateTaskCommand>
{
    public CreateTaskCommandValidator()
    {
        RuleFor(x => x.dto.Title).NotEmpty().MaximumLength(200);
        RuleFor(x => x.dto.Description).MaximumLength(5000);
        RuleFor(x => x.dto.Priority).InclusiveBetween((int)PriorityLevel.Low, (int)PriorityLevel.Critical);
        RuleFor(x => x.dto.EstimatedDuration).InclusiveBetween(1, 10080);
        RuleFor(x => x.dto.Deadline).GreaterThan(DateTime.UtcNow.AddMinutes(-1));
        RuleForEach(x => x.dto.Tags).NotEmpty().MaximumLength(50);
        RuleForEach(x => x.dto.DependencyIds).NotEmpty();
    }
}

public sealed class UpdateTaskCommandValidator : AbstractValidator<UpdateTaskCommand>
{
    public UpdateTaskCommandValidator()
    {
        RuleFor(x => x.dto.Title).MaximumLength(200).When(x => x.dto.Title is not null);
        RuleFor(x => x.dto.Description).MaximumLength(5000).When(x => x.dto.Description is not null);
        RuleFor(x => x.dto.Priority).InclusiveBetween((int)PriorityLevel.Low, (int)PriorityLevel.Critical).When(x => x.dto.Priority.HasValue);
        RuleFor(x => x.dto.Status).InclusiveBetween((int)Shared.Enums.TaskStatus.Pending, (int)Shared.Enums.TaskStatus.OnHold).When(x => x.dto.Status.HasValue);
        RuleFor(x => x.dto.EstimatedDuration).InclusiveBetween(1, 10080).When(x => x.dto.EstimatedDuration.HasValue);
        RuleFor(x => x.dto.Progress).InclusiveBetween(0, 100).When(x => x.dto.Progress.HasValue);
    }
}
