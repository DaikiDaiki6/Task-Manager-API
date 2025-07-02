using FluentValidation;
using TaskManagerAPI.DTOs.Task;

namespace TaskManagerAPI.Validators.Task;

public class UpdateTaskRequestValidator : AbstractValidator<UpdateTaskRequest>
{
    public UpdateTaskRequestValidator() 
    {
        RuleFor(x => x.Title)
            .NotEmpty().WithMessage("Title is required")
            .Length(1, 100).WithMessage("Title must be between 1 and 100 characters");

        RuleFor(x => x.Description)
            .NotEmpty().WithMessage("Description is required")
            .Length(1, 500).WithMessage("Description must be between 1 and 500 characters");

        RuleFor(x => x.DueDate)
            .GreaterThan(DateTime.Now).WithMessage("Due date must be in the future");

        RuleFor(x => x.Status)
            .IsInEnum().WithMessage("Invalid task status");
    }
}