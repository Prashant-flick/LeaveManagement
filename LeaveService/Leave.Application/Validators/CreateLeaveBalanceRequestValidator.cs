using FluentValidation;
using Leave.Application.DTOs;

namespace Leave.Application.Validators;

public class CreateLeaveBalanceRequestValidator : AbstractValidator<CreateLeaveBalanceRequest>
{
    public CreateLeaveBalanceRequestValidator()
    {
        RuleFor(x => x.EmployeeId)
            .NotEmpty()
            .GreaterThan(0)
            .WithMessage("EmployeeId must be valid");

        RuleFor(x => x.TotalLeaves)
            .NotEmpty()
            .GreaterThan(0)
            .WithMessage("Total leaves must be greater than 0");

        RuleFor(x => x.Year)
            .NotEmpty()
            .GreaterThan(1999)
            .LessThan(2027)
            .WithMessage("YEAR Must be greater than 1999 and less than 2027");
    }
}