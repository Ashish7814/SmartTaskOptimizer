using FluentValidation;
using SmartTaskOptimizer.Shared.DTOs.Tasks;
using SmartTaskOptimizer.Shared.Enums;

namespace SmartTaskOptimizer.Application.Validators;

public sealed class CreateTaskDtoValidator : AbstractValidator<CreateTaskDto>
{
    public CreateTaskDtoValidator()
    {
        RuleFor(x => x.Title).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Description).MaximumLength(5000);
        RuleFor(x => x.Priority).InclusiveBetween((int)PriorityLevel.Low, (int)PriorityLevel.Critical);
        RuleFor(x => x.EstimatedDuration).InclusiveBetween(1, 7 * 24 * 60);
        RuleFor(x => x.Deadline).GreaterThan(DateTime.UtcNow.AddMinutes(-1));
        RuleForEach(x => x.Tags).NotEmpty().MaximumLength(50);
    }
}
