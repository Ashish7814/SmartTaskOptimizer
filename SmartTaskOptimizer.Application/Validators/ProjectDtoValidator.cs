using FluentValidation;
using SmartTaskOptimizer.Shared.DTOs.Project;

namespace SmartTaskOptimizer.Application.Validators;

public sealed class UpdateProjectDtoValidator : AbstractValidator<ProjectDto>
{
    public UpdateProjectDtoValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Description).MaximumLength(2000);
    }
}
