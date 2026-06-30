using FluentValidation;

namespace Leave.Application.Features.Leaves.Commands.CreateLeave
{
    public class CreateLeaveCommandValidator : AbstractValidator<CreateLeaveCommand>
    {
        public CreateLeaveCommandValidator()
        {
            RuleFor(x => x.StartDate)
                .LessThanOrEqualTo(x => x.EndDate)
                .WithMessage("Start date must be before or equal to end date");

            RuleFor(x => x.EndDate)
                .GreaterThanOrEqualTo(x => x.StartDate)
                .WithMessage("End date must be after or equal to start date");

            RuleFor(x => x.Reason)
                .NotEmpty()
                .MaximumLength(500)
                .WithMessage("Reason is required and must not exceed 500 characters");
            
            RuleFor(x => x)
                .Must(x => (x.EndDate - x.StartDate).Days <= 30)
                .WithMessage("Leave cannot exceed 30 days");
        }
    }
}
