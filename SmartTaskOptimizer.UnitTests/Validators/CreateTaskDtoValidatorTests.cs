using FluentValidation.TestHelper;
using SmartTaskOptimizer.Application.Validators;
using SmartTaskOptimizer.Shared.DTOs.Tasks;
using Xunit;

namespace SmartTaskOptimizer.UnitTests.Validators;

public sealed class CreateTaskDtoValidatorTests
{
    [Fact]
    public async Task Rejects_empty_title()
    {
        var validator = new CreateTaskDtoValidator();
        var result = await validator.TestValidateAsync(new CreateTaskDto { Title = "", EstimatedDuration = 30, Deadline = DateTime.UtcNow.AddHours(1) });
        Assert.Contains(result.Errors, x => x.PropertyName == "Title");
    }

    [Fact]
    public async Task Accepts_valid_task()
    {
        var validator = new CreateTaskDtoValidator();
        var result = await validator.TestValidateAsync(new CreateTaskDto { Title = "Implement board", EstimatedDuration = 60, Deadline = DateTime.UtcNow.AddHours(1), Priority = 2 });
        Assert.Empty(result.Errors);
    }
}
