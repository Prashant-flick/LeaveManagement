using FluentValidation;

namespace Employee.Application.Features.Employees.Commands.CreateEmployee
{
    public class CreateEmployeeCommandValidator : AbstractValidator<CreateEmployeeCommand>
    {
        public CreateEmployeeCommandValidator()
        {
            RuleFor(x => x.UserId)
                .GreaterThan(0)
                .WithMessage("UserId must be valid");

            RuleFor(x => x.FirstName)
                .NotEmpty()
                .MaximumLength(100);

            RuleFor(x => x.LastName)
                .NotEmpty()
                .MaximumLength(100);

            RuleFor(x => x.Department)
                .NotEmpty()
                .MaximumLength(100);

            RuleForEach(x => x.RoleIds)
                .GreaterThan(0)
                .When(x => x.RoleIds != null)
                .WithMessage("Invalid role id");

            RuleFor(x => x.ManagerId)
                .GreaterThan(0)
                .When(x => x.ManagerId.HasValue)
                .WithMessage("ManagerId must be valid");
        }
    }
}
