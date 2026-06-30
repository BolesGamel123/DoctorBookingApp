using Application.Features.Auth.Commands.Register;
using FluentAssertions;
using Tests.Common;

namespace Tests.Features.Auth;

public sealed class RegisterCommandTests : TestBase
{
    private readonly RegisterCommandHandler _handler;

    public RegisterCommandTests()
    {
        _handler = new RegisterCommandHandler(
            Context, TokenServiceMock.Object, PasswordHasherMock.Object, JwtSettingsOptions);
    }

    [Fact]
    public async Task Handle_ValidCommand_ShouldCreatePatientAndReturnTokens()
    {
        var command = new RegisterCommand(
            "John Doe", "john@example.com", "+201234567890", "Password123", "Password123");

        var result = await _handler.Handle(command, CancellationToken.None);

        result.Success.Should().BeTrue();
        result.Data.Should().NotBeNull();
        result.Data!.Succeeded.Should().BeTrue();
        result.Data.AccessToken.Should().Be("test-access-token");
        result.Data.RefreshToken.Should().Be("test-refresh-token");
        result.Data.Patient.Should().NotBeNull();
        result.Data.Patient!.FullName.Should().Be("John Doe");
        result.Data.Patient.Email.Should().Be("john@example.com");
        Context.patients.Should().HaveCount(1);
    }

    [Fact]
    public async Task Handle_DuplicateEmail_ShouldReturnFailure()
    {
        var firstCommand = new RegisterCommand(
            "John Doe", "john@example.com", "+201111111111", "Password123", "Password123");
        await _handler.Handle(firstCommand, CancellationToken.None);

        var duplicateCommand = new RegisterCommand(
            "Jane Doe", "john@example.com", "+201222222222", "Password123", "Password123");
        var result = await _handler.Handle(duplicateCommand, CancellationToken.None);

        result.Success.Should().BeFalse();
        result.Errors.Should().Contain("Email is already registered.");
        Context.patients.Should().HaveCount(1);
    }

    [Fact]
    public async Task Handle_DuplicateMobile_ShouldReturnFailure()
    {
        var firstCommand = new RegisterCommand(
            "John Doe", "john@example.com", "+201234567890", "Password123", "Password123");
        await _handler.Handle(firstCommand, CancellationToken.None);

        var duplicateCommand = new RegisterCommand(
            "Jane Doe", "jane@example.com", "+201234567890", "Password123", "Password123");
        var result = await _handler.Handle(duplicateCommand, CancellationToken.None);

        result.Success.Should().BeFalse();
        result.Errors.Should().Contain("Mobile number is already registered.");
    }

    [Fact]
    public async Task Handle_ShouldHashPassword()
    {
        var command = new RegisterCommand(
            "John Doe", "john@example.com", "+201234567890", "MySecret123", "MySecret123");

        await _handler.Handle(command, CancellationToken.None);

        var patient = Context.patients.First();
        patient.password.Should().Be("hashed_MySecret123");
        PasswordHasherMock.Verify(x => x.Hash("MySecret123"), Moq.Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldSetRefreshToken()
    {
        var command = new RegisterCommand(
            "John Doe", "john@example.com", "+201234567890", "Password123", "Password123");

        await _handler.Handle(command, CancellationToken.None);

        var patient = Context.patients.First();
        patient.refresh_token.Should().Be("test-refresh-token");
        patient.refresh_token_expiry.Should().BeAfter(DateTime.UtcNow);
    }
}
