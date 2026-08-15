using FluentValidation;
using SmartTaskOptimizer.Shared.DTOs.Project;

namespace SmartTaskOptimizer.Application.Validators;

public sealed class AddProjectMemberDtoValidator : AbstractValidator<AddProjectMemberDto>
{
    public AddProjectMemberDtoValidator()
    {
        RuleFor(x => x.UserId).NotEmpty();
        RuleFor(x => x.Role).NotEmpty().Must(x => x is "Member" or "Manager").WithMessage("Role must be Member or Manager.");
    }
}
