using Application.Features.Auth.Commands.Login;
using Application.Features.Auth.Commands.Register;
using FluentAssertions;
using FluentValidation.TestHelper;

namespace Tests.Features.Auth;

public sealed class AuthValidatorTests
{
    // ── Register Validator ────────────────────────────────────
    private readonly RegisterCommandValidator _registerValidator = new();

    [Fact]
    public void RegisterValidator_ValidCommand_ShouldPass()
    {
        var command = new RegisterCommand(
            "John Doe", "john@example.com", "+201234567890", "Password123", "Password123");
        var result = _registerValidator.TestValidate(command);
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Theory]
    [InlineData("", "john@example.com", "+20123", "Pass123!", "Pass123!")]
    [InlineData("John", "", "+20123", "Pass123!", "Pass123!")]
    [InlineData("John", "not-an-email", "+20123", "Pass123!", "Pass123!")]
    [InlineData("John", "john@example.com", "", "Pass123!", "Pass123!")]
    [InlineData("John", "john@example.com", "+20123", "", "")]
    [InlineData("John", "john@example.com", "+20123", "short", "short")]
    public void RegisterValidator_InvalidData_ShouldFail(
        string name, string email, string mobile, string password, string confirmPassword)
    {
        var command = new RegisterCommand(name, email, mobile, password, confirmPassword);
        var result = _registerValidator.TestValidate(command);
        result.Errors.Should().NotBeEmpty();
    }

    [Fact]
    public void RegisterValidator_PasswordMismatch_ShouldFail()
    {
        var command = new RegisterCommand(
            "John", "john@example.com", "+20123", "Password123", "DifferentPassword");
        var result = _registerValidator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(x => x.ConfirmPassword)
            .WithErrorMessage("Passwords do not match.");
    }

    // ── Login Validator ───────────────────────────────────────
    private readonly LoginCommandValidator _loginValidator = new();

    [Fact]
    public void LoginValidator_ValidCommand_ShouldPass()
    {
        var command = new LoginCommand("john@example.com", "Password123");
        var result = _loginValidator.TestValidate(command);
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void LoginValidator_EmptyEmail_ShouldFail()
    {
        var command = new LoginCommand("", "Password123");
        var result = _loginValidator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(x => x.Email);
    }

    [Fact]
    public void LoginValidator_InvalidEmail_ShouldFail()
    {
        var command = new LoginCommand("not-an-email", "Password123");
        var result = _loginValidator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(x => x.Email);
    }

    [Fact]
    public void LoginValidator_EmptyPassword_ShouldFail()
    {
        var command = new LoginCommand("john@example.com", "");
        var result = _loginValidator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(x => x.Password);
    }
}
