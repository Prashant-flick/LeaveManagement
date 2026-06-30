using FluentValidation;
using System.Linq;

namespace Employee.Application.Features.Employees.Commands.UpdateEmployee
{
    public class UpdateEmployeeCommandValidator : AbstractValidator<UpdateEmployeeCommand>
    {
        public UpdateEmployeeCommandValidator()
        {
            RuleFor(x => x.FirstName)
                .MaximumLength(100)
                .When(x => !string.IsNullOrWhiteSpace(x.FirstName));

            RuleFor(x => x.LastName)
                .MaximumLength(100)
                .When(x => !string.IsNullOrWhiteSpace(x.LastName));

            RuleFor(x => x.Department)
                .MaximumLength(100)
                .When(x => !string.IsNullOrWhiteSpace(x.Department));

            RuleFor(x => x.RoleIds)
                .Must(r => r.Any())
                .When(x => x.RoleIds != null)
                .WithMessage("RoleIds cannot be empty");

            RuleForEach(x => x.RoleIds!)
                .GreaterThan(0)
                .When(x => x.RoleIds != null)
                .WithMessage("Invalid role id");

            RuleFor(x => x.ManagerId)
                .GreaterThan(0)
                .When(x => x.ManagerId.HasValue)
                .WithMessage("ManagerId must be valid");

            RuleFor(x => x.IsActive)
                .NotNull()
                .When(x => x.IsActive.HasValue);

            RuleFor(x => x)
                .Must(x =>
                    !string.IsNullOrWhiteSpace(x.FirstName) ||
                    !string.IsNullOrWhiteSpace(x.LastName) ||
                    !string.IsNullOrWhiteSpace(x.Department) ||
                    (x.RoleIds != null && x.RoleIds.Any()) ||
                    x.ManagerId.HasValue ||
                    x.IsActive.HasValue
                )
                .WithMessage("At least one field must be provided");
        }
    }
}
