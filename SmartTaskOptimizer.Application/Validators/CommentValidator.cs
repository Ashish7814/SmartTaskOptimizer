using FluentValidation;
using SmartTaskOptimizer.Shared.DTOs.Comments;

namespace SmartTaskOptimizer.Application.Validators;

public sealed class CreateTaskCommentDtoValidator : AbstractValidator<CreateTaskCommentDto>
{
    public CreateTaskCommentDtoValidator() => RuleFor(x => x.Body).NotEmpty().MaximumLength(10000);
}
