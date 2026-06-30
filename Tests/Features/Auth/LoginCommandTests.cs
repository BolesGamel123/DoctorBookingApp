using Application.Features.Auth.Commands.Login;
using Domain.Entities;
using FluentAssertions;
using Tests.Common;

namespace Tests.Features.Auth;

public sealed class LoginCommandTests : TestBase
{
    private readonly LoginCommandHandler _handler;

    public LoginCommandTests()
    {
        _handler = new LoginCommandHandler(
            Context, TokenServiceMock.Object, PasswordHasherMock.Object, JwtSettingsOptions);
    }

    private async Task SeedActivePatient()
    {
        Context.patients.Add(new patient
        {
            full_name = "John Doe", email = "john@example.com",
            mobile_number = "+201234567890", password = "hashed_Password123",
            is_active = true, is_banned = false,
            created_at = DateTime.UtcNow, updated_at = DateTime.UtcNow
        });
        await Context.SaveChangesAsync();
    }

    [Fact]
    public async Task Handle_ValidCredentials_ShouldReturnTokens()
    {
        await SeedActivePatient();
        var command = new LoginCommand("john@example.com", "Password123");

        var result = await _handler.Handle(command, CancellationToken.None);

        result.Success.Should().BeTrue();
        result.Data!.Succeeded.Should().BeTrue();
        result.Data.AccessToken.Should().NotBeNullOrEmpty();
        result.Data.Patient!.Email.Should().Be("john@example.com");
    }

    [Fact]
    public async Task Handle_WrongPassword_ShouldFail()
    {
        await SeedActivePatient();
        var command = new LoginCommand("john@example.com", "WrongPassword");

        var result = await _handler.Handle(command, CancellationToken.None);

        result.Success.Should().BeFalse();
        result.Errors.Should().Contain("Invalid email or password.");
    }

    [Fact]
    public async Task Handle_NonExistentEmail_ShouldFail()
    {
        var command = new LoginCommand("nonexistent@example.com", "Password123");

        var result = await _handler.Handle(command, CancellationToken.None);

        result.Success.Should().BeFalse();
        result.Errors.Should().Contain("Invalid email or password.");
    }

    [Fact]
    public async Task Handle_BannedPatient_ShouldFail()
    {
        Context.patients.Add(new patient
        {
            full_name = "Banned User", email = "banned@example.com",
            mobile_number = "+201111111111", password = "hashed_Password123",
            is_active = true, is_banned = true, ban_reason = "Violation",
            created_at = DateTime.UtcNow, updated_at = DateTime.UtcNow
        });
        await Context.SaveChangesAsync();

        var command = new LoginCommand("banned@example.com", "Password123");
        var result = await _handler.Handle(command, CancellationToken.None);

        result.Success.Should().BeFalse();
        result.Errors.Should().Contain("Your account has been suspended.");
    }

    [Fact]
    public async Task Handle_InactivePatient_ShouldFail()
    {
        Context.patients.Add(new patient
        {
            full_name = "Inactive User", email = "inactive@example.com",
            mobile_number = "+201222222222", password = "hashed_Password123",
            is_active = false, is_banned = false,
            created_at = DateTime.UtcNow, updated_at = DateTime.UtcNow
        });
        await Context.SaveChangesAsync();

        var command = new LoginCommand("inactive@example.com", "Password123");
        var result = await _handler.Handle(command, CancellationToken.None);

        result.Success.Should().BeFalse();
        result.Errors.Should().Contain("Your account is not active.");
    }

    [Fact]
    public async Task Handle_SoftDeletedPatient_ShouldFail()
    {
        Context.patients.Add(new patient
        {
            full_name = "Deleted User", email = "deleted@example.com",
            mobile_number = "+201333333333", password = "hashed_Password123",
            is_active = true, is_banned = false, deleted_at = DateTime.UtcNow,
            created_at = DateTime.UtcNow, updated_at = DateTime.UtcNow
        });
        await Context.SaveChangesAsync();

        var command = new LoginCommand("deleted@example.com", "Password123");
        var result = await _handler.Handle(command, CancellationToken.None);

        result.Success.Should().BeFalse();
    }
}
