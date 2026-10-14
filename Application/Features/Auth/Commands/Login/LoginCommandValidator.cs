using FluentValidation;

namespace Application.Features.Auth.Commands.Login;

public sealed class LoginCommandValidator : AbstractValidator<LoginCommand>
{
    public LoginCommandValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email is required.")
            .EmailAddress().WithMessage("A valid email address is required.");

        RuleFor(x => x.Password)
            .NotEmpty().WithMessage("Password is required.")
            .MaximumLength(72).WithMessage("Password must not exceed 72 characters.")
            .Must(x => x is null || System.Text.Encoding.UTF8.GetByteCount(x) <= 72)
            .WithMessage("Password must not exceed 72 UTF-8 bytes.");

        RuleFor(x => x.Email).MaximumLength(254);
        RuleFor(x => x.AccountType).Must(x => x is null || AccountRoles.IsValid(x))
            .WithMessage("AccountType must be Patient, Doctor, or Admin.");
    }
}
