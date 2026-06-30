using Application.Features.Auth.Commands.RefreshToken;
using Application.Features.Auth.Commands.RevokeToken;
using Domain.Entities;
using FluentAssertions;
using Tests.Common;

namespace Tests.Features.Auth;

public sealed class TokenCommandTests : TestBase
{
    [Fact]
    public async Task RefreshToken_ValidToken_ShouldReturnNewTokens()
    {
        // Arrange
        Context.patients.Add(new patient
        {
            full_name = "John Doe", email = "john@example.com",
            mobile_number = "+201234567890", password = "hashed_pw",
            is_active = true, refresh_token = "valid-refresh-token",
            refresh_token_expiry = DateTime.UtcNow.AddDays(7),
            created_at = DateTime.UtcNow, updated_at = DateTime.UtcNow
        });
        await Context.SaveChangesAsync();

        var handler = new RefreshTokenCommandHandler(
            Context, TokenServiceMock.Object, JwtSettingsOptions);
        var command = new RefreshTokenCommand("valid-refresh-token");

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.Success.Should().BeTrue();
        result.Data!.Succeeded.Should().BeTrue();
        result.Data.AccessToken.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task RefreshToken_ExpiredToken_ShouldFail()
    {
        Context.patients.Add(new patient
        {
            full_name = "John Doe", email = "john@example.com",
            mobile_number = "+201234567890", password = "hashed_pw",
            is_active = true, refresh_token = "expired-token",
            refresh_token_expiry = DateTime.UtcNow.AddDays(-1),
            created_at = DateTime.UtcNow, updated_at = DateTime.UtcNow
        });
        await Context.SaveChangesAsync();

        var handler = new RefreshTokenCommandHandler(
            Context, TokenServiceMock.Object, JwtSettingsOptions);
        var result = await handler.Handle(
            new RefreshTokenCommand("expired-token"), CancellationToken.None);

        result.Success.Should().BeFalse();
    }

    [Fact]
    public async Task RefreshToken_InvalidToken_ShouldFail()
    {
        var handler = new RefreshTokenCommandHandler(
            Context, TokenServiceMock.Object, JwtSettingsOptions);
        var result = await handler.Handle(
            new RefreshTokenCommand("nonexistent-token"), CancellationToken.None);

        result.Success.Should().BeFalse();
    }

    [Fact]
    public async Task RevokeToken_ValidUser_ShouldClearRefreshToken()
    {
        Context.patients.Add(new patient
        {
            id = 1, full_name = "John Doe", email = "john@example.com",
            mobile_number = "+201234567890", password = "hashed_pw",
            is_active = true, refresh_token = "some-token",
            refresh_token_expiry = DateTime.UtcNow.AddDays(7),
            created_at = DateTime.UtcNow, updated_at = DateTime.UtcNow
        });
        await Context.SaveChangesAsync();

        var handler = new RevokeTokenCommandHandler(Context);
        var result = await handler.Handle(new RevokeTokenCommand(1), CancellationToken.None);

        result.Success.Should().BeTrue();
        var patient = Context.patients.First();
        patient.refresh_token.Should().BeNull();
        patient.refresh_token_expiry.Should().BeNull();
    }

    [Fact]
    public async Task RevokeToken_NonExistentUser_ShouldFail()
    {
        var handler = new RevokeTokenCommandHandler(Context);
        var result = await handler.Handle(new RevokeTokenCommand(999), CancellationToken.None);

        result.Success.Should().BeFalse();
    }
}
