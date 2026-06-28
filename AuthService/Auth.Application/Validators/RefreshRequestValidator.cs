using FluentValidation;

namespace Auth.Application.Validators;

public class RefreshRequestValidator : AbstractValidator<RefreshRequest>
{
    public RefreshRequestValidator()
    {
        RuleFor(x => x.Token)
            .NotEmpty()
            .WithMessage("Expired access token is required");

        RuleFor(x => x.RefreshToken)
            .NotEmpty()
            .WithMessage("Refresh token is required");
    }
}
