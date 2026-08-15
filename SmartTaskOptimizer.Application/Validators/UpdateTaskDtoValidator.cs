using FluentValidation;
using SmartTaskOptimizer.Shared.DTOs.Tasks;
using SmartTaskOptimizer.Shared.Enums;

namespace SmartTaskOptimizer.Application.Validators;

public sealed class UpdateTaskDtoValidator : AbstractValidator<UpdateTaskDto>
{
    public UpdateTaskDtoValidator()
    {
        RuleFor(x => x.Title).MaximumLength(200).When(x => x.Title is not null);
        RuleFor(x => x.Description).MaximumLength(5000).When(x => x.Description is not null);
        RuleFor(x => x.Priority).InclusiveBetween((int)PriorityLevel.Low, (int)PriorityLevel.Critical).When(x => x.Priority.HasValue);
        RuleFor(x => x.Status).InclusiveBetween((int)Shared.Enums.TaskStatus.Pending, (int)Shared.Enums.TaskStatus.OnHold).When(x => x.Status.HasValue);
        RuleFor(x => x.EstimatedDuration).InclusiveBetween(1, 7 * 24 * 60).When(x => x.EstimatedDuration.HasValue);
        RuleFor(x => x.Progress).InclusiveBetween(0, 100).When(x => x.Progress.HasValue);
        RuleForEach(x => x.Tags).NotEmpty().MaximumLength(50).When(x => x.Tags is not null);
    }
}
